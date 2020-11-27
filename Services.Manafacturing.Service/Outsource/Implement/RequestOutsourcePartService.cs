using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class RequestOutsourcePartService : IOutsourcePartRequestService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public RequestOutsourcePartService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<RequestOutsourcePartService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<long> CreateOutsourcePartRequest(RequestOutsourcePartInfo req)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get cấu hình sinh mã
                int customGenCodeId = 0;
                var currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceRequest, EnumObjectType.OutsourceRequest, 0);

                if (currentConfig == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                }
                var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);
                if (generated == null)
                {
                    throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                }
                customGenCodeId = currentConfig.CustomGenCodeId;

                // Create order
                var order = _mapper.Map<RequestOutsourcePart>(req as RequestOutsourcePartModel);
                order.RequestOutsourcePartCode = generated.CustomCode;

                _manufacturingDBContext.RequestOutsourcePart.Add(order);
                await _manufacturingDBContext.SaveChangesAsync();

                // Create order detail
                var orderDetails = new List<RequestOutsourcePartDetail>();
                foreach (var data in req.RequestOutsourcePartDetail)
                {
                    data.RequestOutsourcePartId = order.RequestOutsourcePartId;
                    data.StatusId = EnumOutsourcePartProcessType.Unprocessed;
                    orderDetails.Add(_mapper.Map<RequestOutsourcePartDetail>(data as RequestOutsourcePartDetailModel));
                }

                await _manufacturingDBContext.RequestOutsourcePartDetail.AddRangeAsync(orderDetails);
                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();

                
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.RequestOutsourcePartId, $"Thêm mới yêu cầu gia công chi tiết {order.RequestOutsourcePartId}", order.JsonSerialize());

                if (customGenCodeId > 0)
                {
                    await _customGenCodeHelperService.ConfirmCode(customGenCodeId);
                }

                return order.RequestOutsourcePartId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<RequestOutsourcePartInfo> GetOutsourcePartRequestExtraInfo(int requestOutsourcePartId = 0)
        {
            var sql = new StringBuilder("SELECT * FROM vRequestOutsourcePartExtractInfo v WHERE v.RequestOutsourcePartId = @RequestOutsourcePartId");

            var parammeters = new List<SqlParameter>();
            parammeters.Add(new SqlParameter("@RequestOutsourcePartId", requestOutsourcePartId));

            var extractInfo = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                    .ConvertData<RequestOutsourcePartDetailInfo>();
            if (extractInfo.Count == 0)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var rs = _mapper.Map<RequestOutsourcePartInfo>(extractInfo[0]);
            rs.Status = GetRequestOutsourcePartStatus(extractInfo);
            rs.RequestOutsourcePartDetail = extractInfo.Where(x => x.RequestOutsourcePartDetailId > 0).ToList();
            return rs;
        }

        private string GetRequestOutsourcePartStatus(List<RequestOutsourcePartDetailInfo> req)
        {
            if (req.Where(x => x.StatusId == EnumOutsourcePartProcessType.Unprocessed).Count() > 0)
                return EnumOutsourcePartProcessType.Unprocessed.GetEnumDescription();
            else if (req.Where(x => x.StatusId == EnumOutsourcePartProcessType.Processing).Count() > 0)
                return EnumOutsourcePartProcessType.Processing.GetEnumDescription();
            else if (req.Where(x => x.StatusId == EnumOutsourcePartProcessType.Processed).Count() > 0)
                return EnumOutsourcePartProcessType.Processed.GetEnumDescription();
            return string.Empty;
        }

        public async Task<bool> UpdateOutsourcePartRequest(int requestOutsourcePartId, RequestOutsourcePartInfo req)
        {
            var order = await _manufacturingDBContext.RequestOutsourcePart.FirstOrDefaultAsync(x => x.RequestOutsourcePartId == requestOutsourcePartId);
            if (order == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest, $"Không tìm thấy yêu cầu gia công có mã là {requestOutsourcePartId}");

            var details = _manufacturingDBContext.RequestOutsourcePartDetail.Where(x => x.RequestOutsourcePartId == requestOutsourcePartId).ToList();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // update order
                _mapper.Map(req, order);

                //Valid Update and action
                foreach (var u in details)
                {
                        var s = req.RequestOutsourcePartDetail.FirstOrDefault(x => x.RequestOutsourcePartDetailId == u.RequestOutsourcePartDetailId);
                    if (s != null)
                        _mapper.Map(s, u);
                    else
                        u.IsDeleted = true;
                }

                // create new detail
                var lsNewDetail = req.RequestOutsourcePartDetail.Where(x => !details.Select(x => x.RequestOutsourcePartDetailId).Contains(x.RequestOutsourcePartDetailId)).ToList();
                var temp = _mapper.Map<List<RequestOutsourcePartDetail>>(lsNewDetail);
                temp.ForEach(x => x.RequestOutsourcePartId = order.RequestOutsourcePartId);
                await _manufacturingDBContext.RequestOutsourcePartDetail.AddRangeAsync(temp);

                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, req.RequestOutsourcePartId, $"Cập nhật yêu cầu gia công chi tiết {req.RequestOutsourcePartId}", req.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<PageData<RequestOutsourcePartDetailInfo>> GetListOutsourcePartRequest(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductName LIKE @Keyword ");
                whereCondition.Append("OR v.RequestOutsourcePartCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductPartName LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }
            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vRequestOutsourcePartExtractInfo", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder("SELECT * FROM vRequestOutsourcePartExtractInfo v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.RequestOutsourcePartDetailId) Total FROM vRequestOutsourcePartExtractInfo v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.RequestOutsourcePartId");

            var table = await _manufacturingDBContext.QueryDataTable(totalSql.ToString(), parammeters.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            if (size >= 0)
            {
                sql.Append(@$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT { size}
                ROWS ONLY");
            }

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<RequestOutsourcePartDetailInfo>().ToList();

            return (lst, total);
        }

        public async Task<bool> DeletedOutsourcePartRequest(int requestOutsourcePartId)
        {
            var order = await _manufacturingDBContext.RequestOutsourcePart.FirstOrDefaultAsync(x => x.RequestOutsourcePartId == requestOutsourcePartId);
            if (order == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);
            var details = await _manufacturingDBContext.RequestOutsourcePartDetail.Where(x => x.RequestOutsourcePartId == order.RequestOutsourcePartId).ToListAsync();

            details.ForEach(x => x.IsDeleted = true);
            order.IsDeleted = true;

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }
    }
}
