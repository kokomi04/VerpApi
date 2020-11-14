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
    public class RequestOutsourcePartService : IRequestOutsourcePartService
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

        public async Task<bool> CreateRequestOutsourcePart(RequestOutsourcePartInfo req)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get cấu hình sinh mã
                var currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.RequestOutsource, 0);
                if (currentConfig == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                }
                var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);
                if (generated == null)
                {
                    throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                }

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
                    data.Status = OutsourcePartProcessType.Unprocessed;
                    orderDetails.Add(_mapper.Map<RequestOutsourcePartDetail>(data as RequestOutsourcePartDetailModel));
                }

                await _manufacturingDBContext.RequestOutsourcePartDetail.AddRangeAsync(orderDetails);
                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();

                await _customGenCodeHelperService.ConfirmCode(EnumObjectType.RequestOutsource, 0);
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.RequestOutsourcePartId, $"Thêm mới yêu cầu gia công chi tiết {order.RequestOutsourcePartId}", order.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<RequestOutsourcePartInfo> GetRequestOutsourcePartExtraInfo(int requestOutsourcePartId = 0)
        {

            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();
            whereCondition.Append("(v.RequestOutsourcePartId = @RequestOutsourcePartId ");
            parammeters.Add(new SqlParameter("@RequestOutsourcePartId", requestOutsourcePartId));

            var sql = new StringBuilder("SELECT * FROM vRequestOutsourcePartDetail v ");
            var data = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                    .ConvertData<RequestOutsourcePartDetailInfo>();
            if(data.Count == 0)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var rs = _mapper.Map<RequestOutsourcePartInfo>(data[0]);
            rs.RequestOutsourcePartDetail = data;
            return rs;
        }

        public async Task<bool> UpdateRequestOutsourcePart(int requestOutsourcePartId, RequestOutsourcePartInfo req)
        {
            var order = _manufacturingDBContext.RequestOutsourcePart.Where(x => x.RequestOutsourcePartId == requestOutsourcePartId).ToList();
            if (order.Count <= 0)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest, $"Không tìm thấy yêu cầu gia công của {requestOutsourcePartId}");

            var details = _manufacturingDBContext.RequestOutsourcePartDetail.Where(x => x.RequestOutsourcePartId == requestOutsourcePartId).ToList();
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var info = _mapper.Map<RequestOutsourcePart>(req as RequestOutsourcePartModel);
                _mapper.Map(info, order);

                var lsDeleteDetail = details.Where(x => !req.RequestOutsourcePartDetail.Select(x => x.RequestOutsourcePartDetailId).Contains(x.RequestOutsourcePartDetailId)).ToList();
                var lsNewDetail = req.RequestOutsourcePartDetail.Where(x => !details.Select(x => x.RequestOutsourcePartDetailId).Contains(x.RequestOutsourcePartDetailId)).ToList();
                var lsUpdateDetail = details.Where(x => req.RequestOutsourcePartDetail.Select(x => x.RequestOutsourcePartDetailId).Contains(x.RequestOutsourcePartDetailId)).ToList();

                //Valid Delete
                lsDeleteDetail.ForEach( x=>{
                    if (x.Status != (int)OutsourcePartProcessType.Unprocessed)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Không thể xóa chi tiết id/{x.RequestOutsourcePartDetailId} vì trạng thái của nó là {((OutsourcePartProcessType)x.Status).GetEnumDescription()}");
                });
                //Valid Update
                lsUpdateDetail.ForEach(x => {
                    if (x.Status != (int)OutsourcePartProcessType.Unprocessed)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Không thể sửa chi tiết id/{x.RequestOutsourcePartDetailId} vì trạng thái của nó là {((OutsourcePartProcessType)x.Status).GetEnumDescription()}");
                });


                //foreach (var d in dData)
                //{
                //    if (d.Status != (int)OutsourcePartProcessType.Unprocessed)
                //        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, "Yêu cầu gia công chi tiết đã có đơn hàng giao công");
                //    d.IsDeleted = true;
                //}
                //foreach (var d in uData)
                //{
                //    if (d.Status != (int)OutsourcePartProcessType.Unprocessed)
                //        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, "Yêu cầu gia công chi tiết đã có đơn hàng giao công");

                //    var s = req.FirstOrDefault(x => x.RequestOutsourcePartId == d.RequestOutsourcePartId);
                //    _mapper.Map(s, d);
                //}
                //await _manufacturingDBContext.SaveChangesAsync();
                //await CreateRequestOutsourcePart(nData);
                //trans.Commit();

                //await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, req.First().ProductionOrderDetailId, $"Cập nhật yêu cầu gia công chi tiết {req.First().RequestOutsourcePartCode}", req.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<PageData<RequestOutsourcePartDetailInfo>> GetListRequestOutsourcePart(string keyword, int page, int size)
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

            var sql = new StringBuilder("SELECT * FROM vRequestOutsourcePartDetail v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.RequestOutsourcePartDetailId) Total FROM vRequestOutsourcePartDetail v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.RequestOutsourcePartDetailId");

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
    }
}
