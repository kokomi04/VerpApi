using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
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
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourcePartRequestService : IOutsourcePartRequestService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public OutsourcePartRequestService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourcePartRequestService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<long> CreateOutsourcePartRequest(OutsourcePartRequestModel model)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // Cấu hình sinh mã
                var ctx = await GenerateOutsouceRequestCode(null, model);

                // Create order
                var request = _mapper.Map<OutsourcePartRequest>(model);
                _manufacturingDBContext.OutsourcePartRequest.Add(request);

                await _manufacturingDBContext.SaveChangesAsync();

                // Create order detail
                var requestDetails = new List<OutsourcePartRequestDetail>();
                foreach (var element in model.Detail)
                {
                    element.OutsourcePartRequestId = request.OutsourcePartRequestId;
                    var entity = _mapper.Map<OutsourcePartRequestDetail>(element);
                    requestDetails.Add(entity);
                }

                await _manufacturingDBContext.OutsourcePartRequestDetail.AddRangeAsync(requestDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                await _manufacturingDBContext.SaveChangesAsync();

                await ctx.ConfirmCode();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, request.OutsourcePartRequestId, $"Thêm mới yêu cầu gia công chi tiết {request.OutsourcePartRequestId}", request.JsonSerialize());

                
                return request.OutsourcePartRequestId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<OutsourcePartRequestModel> GetOutsourcePartRequest(long outsourcePartRequestId = 0)
        {
            var request = await _manufacturingDBContext.OutsourcePartRequest.FirstOrDefaultAsync(x => x.OutsourcePartRequestId == outsourcePartRequestId);
            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var enrichData = await (from p in _manufacturingDBContext.ProductionOrder
                                    join d in _manufacturingDBContext.ProductionOrderDetail on p.ProductionOrderId equals d.ProductionOrderId
                                    where request.ProductionOrderDetailId == d.ProductionOrderDetailId
                                    select new
                                    {
                                        p.ProductionOrderId,
                                        p.ProductionOrderCode,
                                        d.ProductId
                                    }).FirstOrDefaultAsync();

            var details = await _manufacturingDBContext.OutsourcePartRequestDetail.Where(x => x.OutsourcePartRequestId == outsourcePartRequestId)
                .ProjectTo<OutsourcePartRequestDetailModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var purchaseOrders = await _manufacturingDBContext.RefOutsourcePartOrder.Where(x => x.OutsourceRequestId == outsourcePartRequestId).ToListAsync();

            foreach (var element in details)
            {
                element.PurchaseOrder = purchaseOrders.Where(x => x.ProductId == element.ProductId)
                    .Select(x => new PurchaseOrderSimple { PurchaseOrderCode = x.PurchaseOrderCode, PurchaseOrderId = x.PurchaseOrderId })
                    .ToList();
                element.QuantityProcessed = purchaseOrders.Where(x => x.ProductId == element.ProductId).Sum(x => x.PrimaryQuantity);
            }

            var rs = _mapper.Map<OutsourcePartRequestModel>(request);
            rs.Detail = details;
            rs.ProductionOrderCode = enrichData?.ProductionOrderCode;
            rs.RootProductId = enrichData?.ProductId;
            rs.ProductionOrderId = enrichData?.ProductionOrderId;

            return rs;
        }

        public async Task<bool> UpdateOutsourcePartRequest(long OutsourcePartRequestId, OutsourcePartRequestModel model)
        {
            
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var request = await _manufacturingDBContext.OutsourcePartRequest.FirstOrDefaultAsync(x => x.OutsourcePartRequestId == OutsourcePartRequestId);
                if (request == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundRequest, $"Không tìm thấy yêu cầu gia công có mã là {OutsourcePartRequestId}");

                var details = _manufacturingDBContext.OutsourcePartRequestDetail.Where(x => x.OutsourcePartRequestId == OutsourcePartRequestId).ToList();

                // update order
                _mapper.Map(model, request);

                //Valid Update and action
                foreach (var u in details)
                {
                    var s = model.Detail.FirstOrDefault(x => x.OutsourcePartRequestDetailId == u.OutsourcePartRequestDetailId);
                    if (s != null)
                        _mapper.Map(s, u);
                    else
                        u.IsDeleted = true;
                }

                // create new detail
                var newRequestDetails = model.Detail
                    .Where(x => !details.Select(x => x.OutsourcePartRequestDetailId).Contains(x.OutsourcePartRequestDetailId))
                    .AsQueryable()
                    .ProjectTo<OutsourcePartRequestDetail>(_mapper.ConfigurationProvider)
                    .ToList();
                newRequestDetails.ForEach(x => x.OutsourcePartRequestId = request.OutsourcePartRequestId);

                await _manufacturingDBContext.OutsourcePartRequestDetail.AddRangeAsync(newRequestDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, model.OutsourcePartRequestId, $"Cập nhật yêu cầu gia công chi tiết {model.OutsourcePartRequestId}", model.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<PageData<OutsourcePartRequestSearchModel>> Search(string keyword, int page, int size, long fromDate, long toDate, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim().ToLower();

            var query = from r in _manufacturingDBContext.OutsourcePartRequest
                        join rd in _manufacturingDBContext.OutsourcePartRequestDetail on r.OutsourcePartRequestId equals rd.OutsourcePartRequestId
                        join pod in _manufacturingDBContext.ProductionOrderDetail on r.ProductionOrderDetailId equals pod.ProductionOrderDetailId
                        join p1 in _manufacturingDBContext.RefProduct on pod.ProductId equals p1.ProductId into gp1
                        from p1 in gp1.DefaultIfEmpty()
                        join po in _manufacturingDBContext.ProductionOrder on pod.ProductionOrderId equals po.ProductionOrderId
                        join p2 in _manufacturingDBContext.RefProduct on rd.ProductId equals p2.ProductId into gp2
                        from p2 in gp2.DefaultIfEmpty()
                        select new
                        {
                            r.OutsourcePartRequestId,
                            r.OutsourcePartRequestCode,
                            r.CreatedDatetimeUtc,
                            r.MarkInvalid,
                            r.OutsourcePartRequestStatusId,
                            po.ProductionOrderId,
                            po.ProductionOrderCode,
                            pod.OrderCode,
                            RootProductId = pod.ProductId,
                            RootProductCode = p1.ProductCode,
                            RootProductName = p1.ProductName,
                            rd.ProductId,
                            p2.ProductCode,
                            p2.ProductName,
                            rd.OutsourcePartRequestDetailFinishDate
                        };



            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.OrderCode.Contains(keyword)
                            || x.OutsourcePartRequestCode.Contains(keyword)
                            || x.ProductCode.Contains(keyword)
                            || x.ProductName.Contains(keyword)
                            || x.ProductionOrderCode.Contains(keyword)
                            || x.RootProductCode.Contains(keyword)
                            || x.RootProductName.Contains(keyword)
                        );
            }

            if(fromDate > 0 && toDate > 0)
            {
                query = query.Where(x => x.CreatedDatetimeUtc >= fromDate.UnixToDateTime() && x.CreatedDatetimeUtc < toDate.UnixToDateTime().Value.AddDays(1));
            }

            if (filters != null)
            {
                query = query.InternalFilter(filters);
            }

            query = query.OrderByDescending(x => x.CreatedDatetimeUtc);

            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
                        .Select(x => new OutsourcePartRequestSearchModel
                        {
                            CreatedDatetimeUtc = x.CreatedDatetimeUtc.GetUnix(),
                            MarkInvalid = x.MarkInvalid,
                            OrderCode = x.OrderCode,
                            OutsourcePartRequestCode = x.OutsourcePartRequestCode,
                            OutsourcePartRequestDetailFinishDate = x.OutsourcePartRequestDetailFinishDate.GetUnix(),
                            OutsourcePartRequestId = x.OutsourcePartRequestId,
                            OutsourcePartRequestStatusId = x.OutsourcePartRequestStatusId,
                            ProductCode = x.ProductCode,
                            ProductId = x.ProductId,
                            RootProductId = x.RootProductId,
                            ProductionOrderCode = x.ProductionOrderCode,
                            ProductionOrderId = x.ProductionOrderId,
                            ProductName = x.ProductName,
                            RootProductCode = x.RootProductCode,
                            RootProductName = x.RootProductName
                        })
                        .ToListAsync();

            var arrOutsourceRequestId = lst.Select(x => x.OutsourcePartRequestId).Distinct().ToArray();
            var purchaseOrders = await _manufacturingDBContext.RefOutsourcePartOrder.Where(x => arrOutsourceRequestId.Contains(x.OutsourceRequestId.GetValueOrDefault())).ToListAsync();
            foreach (var element in lst)
            {
                element.PurchaseOrder = purchaseOrders.Where(x => x.ProductId == element.ProductId && x.OutsourceRequestId == element.OutsourcePartRequestId)
                    .Select(x => new PurchaseOrderSimple { PurchaseOrderCode = x.PurchaseOrderCode, PurchaseOrderId = x.PurchaseOrderId })
                    .ToList();
            }

            return (lst, total);
        }

        public async Task<bool> DeletedOutsourcePartRequest(long outsourcePartRequestId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var order = await _manufacturingDBContext.OutsourcePartRequest.FirstOrDefaultAsync(x => x.OutsourcePartRequestId == outsourcePartRequestId);
                if (order == null)
                    throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

                var hasPurchaseOrder = await _manufacturingDBContext.RefOutsourcePartOrder.AnyAsync(x=>x.OutsourceRequestId == outsourcePartRequestId);
                if(hasPurchaseOrder)
                    throw new BadRequestException(OutsourceErrorCode.HasPurchaseOrder);
                
                var details = await _manufacturingDBContext.OutsourcePartRequestDetail
                    .Where(x => x.OutsourcePartRequestId == order.OutsourcePartRequestId)
                    .ToListAsync();

                var lst = (from o in _manufacturingDBContext.OutsourceOrder
                           join d in _manufacturingDBContext.OutsourceOrderDetail
                             on o.OutsourceOrderId equals d.OutsourceOrderId
                           where o.OutsourceTypeId == (int)EnumOutsourceType.OutsourcePart
                           select d).GroupBy(x => x.ObjectId).Select(x => new
                           {
                               ObjectId = x.Key,
                               QuantityProcessed = x.Sum(x => x.Quantity)
                           });
                foreach (var detail in details)
                {
                    if (lst.Where(y => y.ObjectId == detail.OutsourcePartRequestDetailId && y.QuantityProcessed > 0).Count() != 0)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Đã có đơn hàng gia công cho yêu cầu {order.OutsourcePartRequestCode}");
                    detail.IsDeleted = true;
                };
                order.IsDeleted = true;

                await _manufacturingDBContext.SaveChangesAsync();


                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeletedOutsourcePartRequest");
                throw;
            }


        }

        public async Task<IList<OutsourcePartRequestDetailInfo>> GetOutsourcePartRequestDetailByProductionOrderId(long productionOrderId)
        {
            var sql = new StringBuilder($"SELECT * FROM vOutsourcePartRequestExtractInfo v WHERE v.ProductionOrderId = {productionOrderId}");
            var resultData = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), Array.Empty<SqlParameter>()))
                .ConvertData<OutsourcePartRequestDetailExtractInfo>()
                .AsQueryable()
                .ProjectTo<OutsourcePartRequestDetailInfo>(_mapper.ConfigurationProvider)
                .ToList();

            return resultData;
        }
        
        public async Task<IList<OutsourcePartRequestOutput>> GetOutsourcePartRequestByProductionOrderId(long productionOrderId)
        {
            var data = await _manufacturingDBContext.OutsourcePartRequest.AsNoTracking()
                                .Include(x => x.ProductionOrderDetail)
                                .Where(x => x.ProductionOrderDetail.ProductionOrderId == productionOrderId)
                                .ProjectTo<OutsourcePartRequestOutput>(_mapper.ConfigurationProvider)
                                .ToListAsync();
            return data;
        }

        public async Task<bool> UpdateOutsourcePartRequestStatus(long[] outsourcePartRequestId)
        {
            var transaction = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var requests = await _manufacturingDBContext.OutsourcePartRequest
                .Where(x => outsourcePartRequestId.Contains(x.OutsourcePartRequestId))
                .ToListAsync();

                foreach (var rq in requests)
                {
                    var arrPurchaseOrderId = await _manufacturingDBContext.RefOutsourcePartOrder.Where(x => x.OutsourceRequestId == rq.OutsourcePartRequestId)
                        .Select(x => x.PurchaseOrderId)
                        .ToListAsync();

                    var totalStatus = (await _manufacturingDBContext.RefOutsourcePartTrack.AsNoTracking()
                        .Where(x => arrPurchaseOrderId.Contains(x.PurchaseOrderId) && x.ProductId.HasValue == false)
                        .ToListAsync())
                        .GroupBy(x => x.PurchaseOrderId)
                        .Select(g => g.OrderByDescending(x => x.PurchaseOrderTrackedId).Take(1).FirstOrDefault()?.Status)
                        .Sum();

                    if (totalStatus.GetValueOrDefault() == 0)
                        rq.OutsourcePartRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed;
                    else
                    {
                        var mapQuantityProcessed = _manufacturingDBContext.RefOutsourcePartOrder
                            .Where(x => x.OutsourceRequestId == rq.OutsourcePartRequestId).ToList()
                            .GroupBy(x => x.ProductId)
                            .ToDictionary(k => k.Key, v => v.Sum(x => x.PrimaryQuantity));

                        var isCheckOrder = false;
                        var details = (from d in _manufacturingDBContext.OutsourcePartRequestDetail
                                       where d.OutsourcePartRequestId == rq.OutsourcePartRequestId
                                       group d by d.ProductId into g
                                       select new
                                       {
                                           ProductId = g.Key,
                                           Quantity = g.Sum(x => x.Quantity)
                                       }).ToList();

                        foreach (var d in details)
                        {
                            mapQuantityProcessed.TryGetValue(d.ProductId, out var quantityProcessed);
                            if (d.Quantity - quantityProcessed != 0)
                            {
                                isCheckOrder = false;
                                break;
                            }

                            isCheckOrder = true;
                        }
                        if (isCheckOrder && (totalStatus.GetValueOrDefault() == ((int)EnumOutsourceTrackStatus.HandedOver * arrPurchaseOrderId.Count())))
                            rq.OutsourcePartRequestStatusId = (int)EnumOutsourceRequestStatusType.Processed;
                        else rq.OutsourcePartRequestStatusId = (int)EnumOutsourceRequestStatusType.Processing;
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                await transaction.TryRollbackTransactionAsync();
                throw ex;
            }
        }

        private async Task<GenerateCodeContext> GenerateOutsouceRequestCode(long? outsourcePartRequestId, OutsourcePartRequestModel model)
        {
            model.OutsourcePartRequestCode = (model.OutsourcePartRequestCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.OutsourceRequest)
                .SetConfigData(outsourcePartRequestId ?? 0, DateTime.UtcNow.GetUnix())
                .TryValidateAndGenerateCode(_manufacturingDBContext.OutsourcePartRequest, model.OutsourcePartRequestCode, (s, code) => s.OutsourcePartRequestId != outsourcePartRequestId && s.OutsourcePartRequestCode == code);

            model.OutsourcePartRequestCode = code;

            return ctx;
        }
    }
}
