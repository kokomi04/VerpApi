using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.Track;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourcePartOrderService : IOutsourcePartOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IOutsourceTrackService _outsourceTrackService;
        private readonly IOutsourcePartRequestService _outsourcePartRequestService;

        public OutsourcePartOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourcePartOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IOutsourceTrackService outsourceTrackService
            , IOutsourcePartRequestService outsourcePartRequestService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _outsourceTrackService = outsourceTrackService;
            _outsourcePartRequestService = outsourcePartRequestService;
        }

        public async Task<long> CreateOutsourceOrderPart(OutsourceOrderInfo req)
        {
            await CheckMarkInvalidOutsourcePartRequest(req.OutsourceOrderDetail.Select(x => x.ObjectId).ToArray());

            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    CustomGenCodeOutputModel currentConfig = null;
                    string outsoureOrderCode = "";
                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceOrder, EnumObjectType.OutsourceOrder, 0, null, req.OutsourceOrderCode, req.OutsourceOrderDate);
                        if (currentConfig == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                        }
                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, req.OutsourceOrderCode, req.OutsourceOrderDate);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }

                        outsoureOrderCode = generated.CustomCode;
                    }
                    else
                    {
                        // Validate unique
                        if (_manufacturingDBContext.OutsourceOrder.Any(o => o.OutsourceOrderCode == req.OutsourceOrderCode))
                            throw new BadRequestException(OutsourceErrorCode.OutsoureOrderCodeAlreadyExisted);
                    }
                    if (!req.OutsourceOrderDate.HasValue)
                    {
                        req.OutsourceOrderDate = DateTime.UtcNow.GetUnix();
                    }

                    var order = _mapper.Map<OutsourceOrder>(req as OutsourceOrderModel);
                    order.OutsourceTypeId = (int)EnumOutsourceType.OutsourcePart;
                    order.OutsourceOrderCode = string.IsNullOrWhiteSpace(order.OutsourceOrderCode) ? outsoureOrderCode : order.OutsourceOrderCode;

                    _manufacturingDBContext.OutsourceOrder.Add(order);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.OutsourceOrderDetail);
                    detail.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manufacturingDBContext.SaveChangesAsync();


                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);
                    }

                    // Tạo lịch sử theo dõi lần đầu
                    await _outsourceTrackService.CreateOutsourceTrack(new OutsourceTrackModel
                    {
                        OutsourceTrackDate = DateTime.Now.GetUnix(),
                        OutsourceTrackDescription = "Tạo đơn hàng",
                        OutsourceTrackStatusId = EnumOutsourceTrackStatus.Created,
                        OutsourceTrackTypeId = EnumOutsourceTrackType.All,
                        OutsourceOrderId = order.OutsourceOrderId
                    });

                    await _manufacturingDBContext.SaveChangesAsync();

                    await UpdateOutsourcePartRequestStatus(detail.Select(x => x.ObjectId));

                    await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.OutsourceOrderId, $"Thêm mới đơn hàng gia công chi tiết {order.OutsourceOrderId}", req.JsonSerialize());

                    await trans.CommitAsync();

                    return order.OutsourceOrderId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError("CreateOutsourceOrderPart");
                    throw ex;
                }
            }
        }

        public async Task<bool> DeleteOutsourceOrderPart(long outsourceOrderId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var outsourceOrder = await _manufacturingDBContext.OutsourceOrder.SingleOrDefaultAsync(o => o.OutsourceOrderId == outsourceOrderId);
                if (outsourceOrder == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);
                var outsourceOrderDetail = await _manufacturingDBContext.OutsourceOrderDetail.Where(o => o.OutsourceOrderId == outsourceOrderId).ToListAsync();

                outsourceOrder.IsDeleted = true;
                outsourceOrderDetail.ForEach(x => x.IsDeleted = true);

                await _manufacturingDBContext.SaveChangesAsync();

                await UpdateOutsourcePartRequestStatus(outsourceOrderDetail.Select(x => x.ObjectId));

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceOrder.OutsourceOrderId, $"Loại bỏ đơn hàng gia công chi tiết {outsourceOrder.OutsourceOrderId}", outsourceOrder.JsonSerialize());

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "DeleteOutsourceOrderPart");
                throw;
            }
        }

        public async Task<PageData<OutsourcePartOrderDetailInfo>> GetListOutsourceOrderPart(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();
            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append(" (v.OutsourceOrderCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductPartName LIKE @Keyword ");
                whereCondition.Append("OR v.ProductTitle LIKE @Keyword ");
                whereCondition.Append("OR v.RequestOutsourcePartCode LIKE @Keyword ");
                whereCondition.Append("OR v.OrderCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductionOrderCode LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }
            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vOutsourceOrderExtractInfo", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder("SELECT * FROM vOutsourcePartOrderDetailExtractInfo v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.OutsourceOrderDetailId) Total FROM vOutsourcePartOrderDetailExtractInfo v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }

            sql.Append($" ORDER BY v.OutsourceOrderId");

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
            var lst = resultData.ConvertData<OutsourcePartOrderDetailExtractInfo>().AsQueryable().ProjectTo<OutsourcePartOrderDetailInfo>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }

        public async Task<OutsourceOrderInfo> GetOutsourceOrderPart(long outsourceOrderId)
        {
            var outsourceOrder = await _manufacturingDBContext.OutsourceOrder
                .ProjectTo<OutsourceOrderInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(o => o.OutsourceOrderId == outsourceOrderId);

            if (outsourceOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var filter = new SingleClause
            {
                DataType = EnumDataType.BigInt,
                FieldName = "OutsourceOrderId",
                Operator = EnumOperator.Equal,
                Value = outsourceOrder.OutsourceOrderId
            };

            var data = (await GetListOutsourceOrderPart(string.Empty, 1, 9999, filter)).List;
            foreach (var item in data)
            {
                var outsourceOrderDetail = _mapper.Map<OutsourceOrderDetailInfo>(item);
                if (outsourceOrderDetail.OutsourceOrderDetailId > 0)
                    outsourceOrder.OutsourceOrderDetail.Add(outsourceOrderDetail);
            }

            return outsourceOrder;
        }

        public async Task<bool> UpdateOutsourceOrderPart(long outsourceOrderId, OutsourceOrderInfo req)
        {
             await CheckMarkInvalidOutsourcePartRequest(req.OutsourceOrderDetail.Select(x => x.ObjectId).ToArray());

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var outsourceOrder = await _manufacturingDBContext.OutsourceOrder.SingleOrDefaultAsync(o => o.OutsourceOrderId == outsourceOrderId);
                if (outsourceOrder == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

                var outsourceOrderDetail = await _manufacturingDBContext.OutsourceOrderDetail.Where(o => o.OutsourceOrderId == outsourceOrderId).ToListAsync();

                //Update order
                _mapper.Map(req, outsourceOrder);

                //update detail
                foreach (var u in outsourceOrderDetail)
                {
                    var s = req.OutsourceOrderDetail.FirstOrDefault(x => x.OutsourceOrderDetailId == u.OutsourceOrderDetailId);
                    if (s != null)
                        _mapper.Map(s, u);
                    else
                        u.IsDeleted = true;
                }

                var lsNewDetail = req.OutsourceOrderDetail.Where(x => !outsourceOrderDetail.Select(x => x.OutsourceOrderDetailId).Contains(x.OutsourceOrderDetailId)).ToList();
                lsNewDetail.ForEach(x => x.OutsourceOrderId = outsourceOrder.OutsourceOrderId);
                var temp = _mapper.Map<List<OutsourceOrderDetail>>(lsNewDetail);

                await _manufacturingDBContext.OutsourceOrderDetail.AddRangeAsync(temp);
                await _manufacturingDBContext.SaveChangesAsync();

                var objectIds = outsourceOrderDetail.Select(x => x.ObjectId).ToList();
                objectIds.AddRange(lsNewDetail.Select(x => x.ObjectId));

                await UpdateOutsourcePartRequestStatus(objectIds);

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceOrder.OutsourceOrderId, $"Cập nhật đơn hàng gia công chi tiết {outsourceOrder.OutsourceOrderId}", outsourceOrder.JsonSerialize());

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "UpdateOutsourceOrderPart");
                throw;
            }
        }


        private async Task CheckMarkInvalidOutsourcePartRequest(long[] outsourcePartrequestDetaildIds)
        {
            var lsInValid = (await _manufacturingDBContext.OutsourcePartRequestDetail.AsNoTracking()
               .Include(x => x.OutsourcePartRequest)
               .Where(x => outsourcePartrequestDetaildIds.Contains(x.OutsourcePartRequestDetailId))
               .ToListAsync())
               .Select(x => new
               {
                   OutsourcePartRequestCode = x.OutsourcePartRequest.OutsourcePartRequestCode,
                   MarkInvalid = x.OutsourcePartRequest.MarkInvalid
               })
               .Where(x => x.MarkInvalid)
               .Select(x => x.OutsourcePartRequestCode)
               .Distinct()
               .ToArray();
            if (lsInValid.Length > 0)
                throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"YCGC \"{String.Join(", ", lsInValid)}\" chưa xác thực với QTSX");
            
        }

        private async Task UpdateOutsourcePartRequestStatus(IEnumerable<long> ObjectIds) {
            var stepIds = await _manufacturingDBContext.OutsourcePartRequestDetail.AsNoTracking()
                .Where(x => ObjectIds.Contains(x.OutsourcePartRequestDetailId))
                .Select(x => x.OutsourcePartRequestId)
                .Distinct()
                .ToArrayAsync();
            await _outsourcePartRequestService.UpdateOutsourcePartRequestStatus(stepIds);
        }
    }
}
