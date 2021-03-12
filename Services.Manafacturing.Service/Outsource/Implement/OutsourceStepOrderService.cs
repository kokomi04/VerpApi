using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
using VErp.Services.Manafacturing.Service.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceStepOrderService : IOutsourceStepOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IOutsourceStepRequestService _outsourceStepRequestService;
        private readonly IOutsourceTrackService _outsourceTrackService;

        public OutsourceStepOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IOutsourceStepRequestService outsourceStepRequestService
            , IOutsourceTrackService outsourceTrackService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _outsourceStepRequestService = outsourceStepRequestService;
            _outsourceTrackService = outsourceTrackService;
        }

        public async Task<long> CreateOutsourceStepOrderPart(OutsourceStepOrderModel req)
        {
            await CheckMarkInvalidOutsourceStepRequest(req);
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    int customGenCodeId = 0;
                    string outsoureOrderCode = "";

                    CustomGenCodeOutputModel currentConfig = null;

                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceOrder, EnumObjectType.OutsourceOrder, 0, null, null, null);
                        if (currentConfig == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                        }
                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, null, null);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }
                        customGenCodeId = currentConfig.CustomGenCodeId;
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
                    order.OutsourceTypeId = (int)EnumOutsourceType.OutsourceStep;
                    order.OutsourceOrderCode = string.IsNullOrWhiteSpace(order.OutsourceOrderCode) ? outsoureOrderCode : order.OutsourceOrderCode;

                    _manufacturingDBContext.OutsourceOrder.Add(order);
                    await _manufacturingDBContext.SaveChangesAsync();

                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.outsourceOrderDetail.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output));
                    detail.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manufacturingDBContext.SaveChangesAsync();

                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);
                    }

                    // Tạo lịch sử theo dõi lần đầu
                    await _outsourceTrackService.CreateOutsourceTrack(new OutsourceTrackModel{
                        OutsourceTrackDate = DateTime.Now.GetUnix(),
                        OutsourceTrackDescription = "Tạo đơn hàng",
                        OutsourceTrackStatusId = EnumOutsourceTrackStatus.Created,
                        OutsourceTrackTypeId = EnumOutsourceTrackType.All,
                        OutsourceOrderId = order.OutsourceOrderId
                    });

                    await _manufacturingDBContext.SaveChangesAsync();

                    await UpdateOutsourceStepRequestStatus(detail.Select(x => x.ObjectId));

                    await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.OutsourceOrderId, $"Thêm mới đơn hàng gia công công đoạn {order.OutsourceOrderCode}", req.JsonSerialize());

                    await trans.CommitAsync();

                    return order.OutsourceOrderId;
                }
                catch (Exception ex)
                {
                    await trans.TryRollbackTransactionAsync();
                    _logger.LogError(ex,"CreateOutsourceStepOrderPart");
                    throw ;
                }
            }
        }

        public async Task<PageData<OutsourceStepOrderSeach>> SearchOutsourceStepOrder(string keyword, int page, int size)
        {
            var outsourceStepOrders = (from o in _manufacturingDBContext.OutsourceOrder
                                       join d in _manufacturingDBContext.OutsourceOrderDetail on o.OutsourceOrderId equals d.OutsourceOrderId
                                       join rd in _manufacturingDBContext.OutsourceStepRequestData on d.ObjectId equals rd.ProductionStepLinkDataId
                                       join r in _manufacturingDBContext.OutsourceStepRequest on rd.OutsourceStepRequestId equals r.OutsourceStepRequestId
                                       where o.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep
                                       group new { o, r, rd, d } by new { o.OutsourceOrderId, o.OutsourceOrderCode, r.OutsourceStepRequestId, r.OutsourceStepRequestCode, o.OutsourceOrderFinishDate } into g
                                       select new OutsourceStepOrderSeach
                                       {
                                           OutsourceOrderFinishDate = g.Key.OutsourceOrderFinishDate.GetUnix(),
                                           OutsourceOrderId = g.Key.OutsourceOrderId,
                                           OutsourceOrderCode = g.Key.OutsourceOrderCode,
                                           OutsourceStepRequestCode = g.Key.OutsourceStepRequestCode,
                                           OutsourceStepRequestId = g.Key.OutsourceStepRequestId,
                                       }).ToList();
            var outsourceStepRequests = (await _outsourceStepRequestService.SearchOutsourceStepRequest(string.Empty, 1, -1, string.Empty, true)).List;

            var data = from order in outsourceStepOrders
                       join request in outsourceStepRequests
                            on order.OutsourceStepRequestId equals request.OutsourceStepRequestId
                       select new OutsourceStepOrderSeach
                       {
                           OutsourceOrderId = order.OutsourceOrderId,
                           OutsourceStepRequestId = order.OutsourceStepRequestId,
                           OrderCode = request.OrderCode,
                           OutsourceOrderCode = order.OutsourceOrderCode,
                           OutsourceOrderFinishDate = order.OutsourceOrderFinishDate,
                           OutsourceStepRequestCode = order.OutsourceStepRequestCode,
                           ProductionOrderCode = request.ProductionOrderCode,
                           ProductionStepTitle = String.Join(", ", request.ProductionSteps.Select(x=>x.Title))
                       };

            if (!string.IsNullOrWhiteSpace(keyword))
                data = data.Where(x => x.ProductionOrderCode.Contains(keyword)
                                   || x.OutsourceOrderCode.Contains(keyword)
                                   || x.OutsourceStepRequestCode.Contains(keyword)
                                   || x.OrderCode.Contains(keyword));

            var total = data.Count();

            return (data.Skip((page - 1) * size).Take(size).ToList(), total);
        }

        public async Task<OutsourceStepOrderModel> GetOutsourceStepOrder(long outsourceStepOrderId)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder.AsNoTracking()
                .Include(x => x.OutsourceOrderDetail)
                .ProjectTo<OutsourceStepOrderModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceStepOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);
            var requestDatas = (from d in outsourceStepOrder.outsourceOrderDetail
                                join rd in _manufacturingDBContext.OutsourceStepRequestData
                                on d.ProductionStepLinkDataId equals rd.ProductionStepLinkDataId
                                select rd).GroupBy(x => x.OutsourceStepRequestId);

            var outsourceOrderDetail = new List<OutsourceStepOrderDetailModel>();
            foreach (var group in requestDatas)
            {
                var outsourceStepRequestDatas = (await _outsourceStepRequestService.GetOutsourceStepRequestData(group.Key)).OrderByDescending(x => x.ProductionStepLinkDataRoleTypeId);

                var percent = decimal.Zero;
                var lst = new List<OutsourceStepOrderDetailModel>();
                foreach (var data in outsourceStepRequestDatas)
                {
                    var detail = outsourceStepOrder.outsourceOrderDetail
                                                   .FirstOrDefault(x => x.ProductionStepLinkDataId == data.ProductionStepLinkDataId);

                    if (detail != null)
                        percent = (decimal)(detail.OutsourceOrderQuantity / data.OutsourceStepRequestDataQuantity);

                    lst.Add(new OutsourceStepOrderDetailModel
                    {
                        OutsourceStepRequestDataQuantity = data.OutsourceStepRequestDataQuantity,
                        OutsourceOrderDetailId = detail == null ? 0 : detail.OutsourceOrderDetailId,
                        OutsourceOrderQuantity = detail == null ? (decimal)(percent * data.OutsourceStepRequestDataQuantity) : detail.OutsourceOrderQuantity,
                        OutsourceOrderId = detail == null ? 0 : detail.OutsourceOrderId,
                        OutsourceOrderPrice = detail == null ? 0 : detail.OutsourceOrderPrice,
                        OutsourceOrderTax = detail == null ? 0 : detail.OutsourceOrderTax,
                        OutsourceStepRequestCode = data.OutsourceStepRequestCode,
                        OutsourceStepRequestDataQuantityProcessed = data.OutsourceStepRequestDataQuantityProcessed,
                        OutsourceStepRequestId = data.OutsourceStepRequestId,
                        ProductionStepId = data.ProductionStepId,
                        ProductionStepLinkDataId = data.ProductionStepLinkDataId,
                        ProductionStepLinkDataQuantity = data.ProductionStepLinkDataQuantity,
                        ProductionStepLinkDataRoleTypeId = data.ProductionStepLinkDataRoleTypeId,
                        ProductionStepLinkDataTitle = data.ProductionStepLinkDataTitle,
                        ProductionStepTitle = data.ProductionStepTitle,
                        OutsourceStepRequestFinishDate = data.OutsourceStepRequestFinishDate,
                        ProductionOrderCode = data.ProductionOrderCode
                    });
                }
                outsourceOrderDetail.AddRange(lst.OrderBy(x => x.ProductionStepId));
            }

            outsourceStepOrder.outsourceOrderDetail = outsourceOrderDetail;
            return outsourceStepOrder;
        }

        public async Task<bool> UpdateOutsourceStepOrder(long outsourceStepOrderId, OutsourceStepOrderModel req)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder
              .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceStepOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            await CheckMarkInvalidOutsourceStepRequest(req);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                //order
                _mapper.Map(req, outsourceStepOrder);
                await _manufacturingDBContext.SaveChangesAsync();

                //detail
                var outsourceStepOrderDetail = await _manufacturingDBContext.OutsourceOrderDetail
                                                        .Where(x => x.OutsourceOrderId == outsourceStepOrder.OutsourceOrderId)
                                                        .ToListAsync();
                foreach (var detail in outsourceStepOrderDetail)
                {
                    var uDetail = req.outsourceOrderDetail.FirstOrDefault(x => x.OutsourceOrderDetailId == detail.OutsourceOrderDetailId);
                    if (uDetail != null)
                        _mapper.Map(uDetail, detail);
                    else detail.IsDeleted = true;
                }

                var newOutsourceStepOrderDetail = req.outsourceOrderDetail
                    .AsQueryable()
                    .Where(x => x.OutsourceOrderDetailId <= 0 && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                    .ProjectTo<OutsourceOrderDetail>(_mapper.ConfigurationProvider)
                    .ToList();
                newOutsourceStepOrderDetail.ForEach(x => x.OutsourceOrderId = outsourceStepOrder.OutsourceOrderId);

                await _manufacturingDBContext.OutsourceOrderDetail.AddRangeAsync(newOutsourceStepOrderDetail);
                await _manufacturingDBContext.SaveChangesAsync();

                var objectIds = outsourceStepOrderDetail.Select(x => x.ObjectId).ToList();
                objectIds.AddRange(newOutsourceStepOrderDetail.Select(x => x.ObjectId));

                await UpdateOutsourceStepRequestStatus(objectIds);

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceStepOrder.OutsourceOrderId, $"Cập nhật đơn hàng gia công công đoạn {outsourceStepOrder.OutsourceOrderCode}", req.JsonSerialize());

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateOutsourceStepOrder");
                throw;
            }
        }

        public async Task<bool> DeleteOutsouceStepOrder(long outsourceStepOrderId)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder
              .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceStepOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                outsourceStepOrder.IsDeleted = true;

                var outsourceStepOrderDetail = await _manufacturingDBContext.OutsourceOrderDetail
                                                        .Where(x => x.OutsourceOrderId == outsourceStepOrder.OutsourceOrderId)
                                                        .ToListAsync();
                outsourceStepOrderDetail.ForEach(x => x.IsDeleted = true);
                await _manufacturingDBContext.SaveChangesAsync();

                await UpdateOutsourceStepRequestStatus(outsourceStepOrderDetail.Select(x => x.ObjectId));

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceStepOrder.OutsourceOrderId, $"Loại bỏ đơn hàng gia công công đoạn {outsourceStepOrder.OutsourceOrderCode}", outsourceStepOrder.JsonSerialize());

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeleteOutsouceStepOrder");
                throw;
            }
        }

        private async Task CheckMarkInvalidOutsourceStepRequest(OutsourceStepOrderModel req)
        {
            var requestIds = req.outsourceOrderDetail.Select(x => x.OutsourceStepRequestId).Distinct();
            var outsourceStepRequests = (await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Where(x => requestIds.Contains(x.OutsourceStepRequestId) && x.IsInvalid)
                .Select(x => x.OutsourceStepRequestCode)
                .ToListAsync());
            if (outsourceStepRequests.Count > 0)
                throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"YCGC \"{String.Join(", ", outsourceStepRequests)}\" chưa xác thực với QTSX");

        }

        private async Task UpdateOutsourceStepRequestStatus(IEnumerable<long> ObjectIds) {
            var stepIds = await _manufacturingDBContext.OutsourceStepRequestData.AsNoTracking()
                .Where(x => ObjectIds.Contains(x.ProductionStepLinkDataId))
                .Select(x => x.OutsourceStepRequestId)
                .Distinct()
                .ToArrayAsync();
            await _outsourceStepRequestService.UpdateOutsourceStepRequestStatus(stepIds);
        }
    }
}
