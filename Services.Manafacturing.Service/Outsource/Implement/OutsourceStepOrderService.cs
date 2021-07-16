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
using OutsourceOrderMaterialsEnity = VErp.Infrastructure.EF.ManufacturingDB.OutsourceOrderMaterials;

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
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductHelperService _productHelperService;

        public OutsourceStepOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IOutsourceStepRequestService outsourceStepRequestService
            , ICurrentContextService currentContextService
            , IProductHelperService productHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _outsourceStepRequestService = outsourceStepRequestService;
            _currentContextService = currentContextService;
            _productHelperService = productHelperService;
        }

        public async Task<long> CreateOutsourceStepOrder(OutsourceStepOrderInput req)
        {
            await CheckMarkInvalidOutsourceStepRequest(req.OutsourceOrderDetail.Select(x => x.ProductionStepLinkDataId).ToArray());
            using (var trans = _manufacturingDBContext.Database.BeginTransaction())
            {
                try
                {
                    string outsoureOrderCode = "";
                    CustomGenCodeOutputModel currentConfig = null;
                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                    {
                        currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceOrder, EnumObjectType.OutsourceOrder, 0, null, null, DateTime.UtcNow.GetUnix());
                        if (currentConfig == null)
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");

                        bool isFirst = true;
                        do
                        {
                            if (!isFirst) await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                            var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, null, DateTime.UtcNow.GetUnix());
                            if (generated == null)
                                throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");

                            outsoureOrderCode = generated.CustomCode;
                            isFirst = false;
                        } while (_manufacturingDBContext.OutsourceOrder.Any(o => o.OutsourceOrderCode == outsoureOrderCode));
                    }

                    if (!req.OutsourceOrderDate.HasValue)
                        req.OutsourceOrderDate = DateTime.Now.Date.GetUnixUtc(_currentContextService.TimeZoneOffset);

                    var order = _mapper.Map<OutsourceOrder>(req);
                    order.OutsourceTypeId = (int)EnumOutsourceType.OutsourceStep;
                    order.OutsourceOrderCode = string.IsNullOrWhiteSpace(order.OutsourceOrderCode) ? outsoureOrderCode : order.OutsourceOrderCode;

                    _manufacturingDBContext.OutsourceOrder.Add(order);
                    await _manufacturingDBContext.SaveChangesAsync();

                    // Danh sách chi tiết gia công
                    var detail = _mapper.Map<List<OutsourceOrderDetail>>(req.OutsourceOrderDetail);
                    detail.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderDetail.AddRange(detail);
                    await _manufacturingDBContext.SaveChangesAsync();

                    // Danh sách các nguyên vật liệu
                    var materials = _mapper.Map<List<OutsourceOrderMaterialsEnity>>(req.OutsourceOrderMaterials);
                    materials.ForEach(x => x.OutsourceOrderId = order.OutsourceOrderId);

                    _manufacturingDBContext.OutsourceOrderMaterials.AddRange(materials);
                    await _manufacturingDBContext.SaveChangesAsync();


                    if (string.IsNullOrWhiteSpace(req.OutsourceOrderCode))
                        await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                    // Tạo lịch sử theo dõi lần đầu
                    await CreateOutsourceTrackFirst(order.OutsourceOrderId);

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

        private async Task<bool> CreateOutsourceTrackFirst(long outsourceOrderId)
        {
            var track = new OutsourceTrackModel
            {
                OutsourceTrackDate = DateTime.Now.GetUnix(),
                OutsourceTrackDescription = "Tạo đơn hàng",
                OutsourceTrackStatusId = EnumOutsourceTrackStatus.Created,
                OutsourceTrackTypeId = EnumOutsourceTrackType.All,
                OutsourceOrderId = outsourceOrderId
            };

            await _manufacturingDBContext.OutsourceTrack.AddAsync(_mapper.Map<OutsourceTrack>(track));
            await _manufacturingDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<PageData<OutsourceStepOrderSeach>> SearchOutsourceStepOrder(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            var outsourceStepOrders = (from o in _manufacturingDBContext.OutsourceOrder
                                       join d in _manufacturingDBContext.OutsourceOrderDetail on o.OutsourceOrderId equals d.OutsourceOrderId
                                       join rd in _manufacturingDBContext.OutsourceStepRequestData on d.ObjectId equals rd.ProductionStepLinkDataId
                                       join r in _manufacturingDBContext.OutsourceStepRequest on rd.OutsourceStepRequestId equals r.OutsourceStepRequestId
                                       where o.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep
                                       group new { o, r, rd, d } by new { o.OutsourceOrderId, o.OutsourceOrderCode, r.OutsourceStepRequestId, r.OutsourceStepRequestCode, o.OutsourceOrderFinishDate, o.OutsourceOrderDate } into g
                                       select new OutsourceStepOrderSeach
                                       {
                                           OutsourceOrderFinishDate = g.Key.OutsourceOrderFinishDate.GetUnix(),
                                           OutsourceOrderId = g.Key.OutsourceOrderId,
                                           OutsourceOrderCode = g.Key.OutsourceOrderCode,
                                           OutsourceStepRequestCode = g.Key.OutsourceStepRequestCode,
                                           OutsourceStepRequestId = g.Key.OutsourceStepRequestId,
                                           OutsourceOrderDate = g.Key.OutsourceOrderDate.GetUnix()
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
                           OutsourceOrderDate = order.OutsourceOrderDate
                       };

            var queryFilter = data.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                queryFilter = queryFilter.Where(x => x.ProductionOrderCode.Contains(keyword)
                                   || x.OutsourceOrderCode.Contains(keyword)
                                   || x.OutsourceStepRequestCode.Contains(keyword)
                                   || x.OrderCode.Contains(keyword));

            if (filters != null)
                queryFilter = queryFilter.InternalFilter(filters);

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
                queryFilter = queryFilter.InternalOrderBy(orderByFieldName, asc);

            var total = queryFilter.Count();
            var lst = (size > 0 ? queryFilter.Skip((page - 1) * size).Take(size) : queryFilter).ToList();

            return (lst, total);
        }

        public async Task<OutsourceStepOrderOutput> GetOutsourceStepOrder(long outsourceStepOrderId)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder.AsNoTracking()
                .ProjectTo<OutsourceStepOrderOutput>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceStepOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var materials = await _manufacturingDBContext.OutsourceOrderMaterials.AsNoTracking()
                .Where(x => x.OutsourceOrderId == outsourceStepOrderId)
                .ProjectTo<OutsourceOrderMaterialsOutput>(_mapper.ConfigurationProvider)
                .ToListAsync();
            var details = await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                .Where(x => x.OutsourceOrderId == outsourceStepOrderId)
                .ProjectTo<OutsourceStepOrderDetailOutput>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var lsLinkDataId = details.Select(x => x.ProductionStepLinkDataId).ToList();
            lsLinkDataId.AddRange(materials.Select(x => x.ProductionStepLinkDataId.GetValueOrDefault()));

            var lsProductInfo = await _productHelperService.GetListProducts(materials.Select(x => (int)x.ProductId).ToList());
            var lsOutsourceStepRequestData = await _outsourceStepRequestService.GetOutsourceStepRequestData(lsLinkDataId.Where(x => x > 0).ToArray());

            foreach (var m in materials)
            {
                var productInfo = lsProductInfo.FirstOrDefault(x => x.ProductId == m.ProductId);
                var data = lsOutsourceStepRequestData.FirstOrDefault(x => x.ProductionStepLinkDataId == m.ProductionStepLinkDataId);
                if (productInfo != null)
                {
                    m.ProductTitle = $"{productInfo.ProductCode}/ {productInfo.ProductName}";
                    m.UnitId = productInfo.UnitId;
                    m.DecimalPlace = productInfo.StockInfo?.UnitConversions?.FirstOrDefault()?.DecimalPlace;
                }

                if (data != null)
                {
                    m.OutsourceRequestCode = data.OutsourceStepRequestCode;
                    m.QuantityRequirement = data.OutsourceStepRequestDataQuantity;
                }
                else
                {
                    var request = lsOutsourceStepRequestData.FirstOrDefault(x => x.OutsourceStepRequestId == m.OutsourceRequestId);
                    m.OutsourceRequestCode = request != null ? request.OutsourceStepRequestCode : string.Empty;
                }
            }

            foreach (var d in details)
            {
                var data = lsOutsourceStepRequestData.FirstOrDefault(x => x.ProductionStepLinkDataId == d.ProductionStepLinkDataId);

                if (data != null)
                {
                    d.OutsourceStepRequestCode = data.OutsourceStepRequestCode;
                    d.OutsourceStepRequestDataQuantity = data.OutsourceStepRequestDataQuantity - (data.OutsourceStepRequestDataQuantityProcessed  - d.OutsourceOrderQuantity);
                    d.OutsourceStepRequestId = data.OutsourceStepRequestId;
                    d.ProductionStepLinkDataTitle = data.ProductionStepLinkDataTitle;
                    d.ProductionStepLinkDataUnitId = data.ProductionStepLinkDataUnitId;
                    d.OutsourceStepRequestFinishDate = data.OutsourceStepRequestFinishDate;
                    d.IsImportant = data.IsImportant;
                    d.ProductionStepTitle = data.ProductionStepTitle;
                    d.DecimalPlace = data.DecimalPlace;
                }
            }

            outsourceStepOrder.OutsourceOrderDetail = details;
            outsourceStepOrder.OutsourceOrderMaterials = materials;

            return outsourceStepOrder;
        }

        public async Task<bool> UpdateOutsourceStepOrder(long outsourceStepOrderId, OutsourceStepOrderOutput req)
        {
            var outsourceStepOrder = await _manufacturingDBContext.OutsourceOrder
              .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceStepOrderId);
            if (outsourceStepOrder == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            await CheckMarkInvalidOutsourceStepRequest(req.OutsourceOrderDetail.Select(x=>x.ProductionStepLinkDataId).ToArray());

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                //order
                _mapper.Map(req, outsourceStepOrder);
                await _manufacturingDBContext.SaveChangesAsync();

                //detail
                var details = await _manufacturingDBContext.OutsourceOrderDetail
                                                        .Where(x => x.OutsourceOrderId == outsourceStepOrder.OutsourceOrderId)
                                                        .ToListAsync();
                foreach (var detail in details)
                {
                    var uDetail = req.OutsourceOrderDetail.FirstOrDefault(x => x.OutsourceOrderDetailId == detail.OutsourceOrderDetailId);
                    if (uDetail != null)
                        _mapper.Map(uDetail, detail);
                    else detail.IsDeleted = true;
                }

                var newOutsourceStepOrderDetail = req.OutsourceOrderDetail
                    .AsQueryable()
                    .Where(x => x.OutsourceOrderDetailId <= 0)
                    .ProjectTo<OutsourceOrderDetail>(_mapper.ConfigurationProvider)
                    .ToList();
                newOutsourceStepOrderDetail.ForEach(x => x.OutsourceOrderId = outsourceStepOrder.OutsourceOrderId);

                await _manufacturingDBContext.OutsourceOrderDetail.AddRangeAsync(newOutsourceStepOrderDetail);
                await _manufacturingDBContext.SaveChangesAsync();

                //materials
                var materials = await _manufacturingDBContext.OutsourceOrderMaterials
                                                        .Where(x => x.OutsourceOrderId == outsourceStepOrder.OutsourceOrderId)
                                                        .ToListAsync();

                foreach (var m in materials)
                {
                    var s = req.OutsourceOrderMaterials.FirstOrDefault(x => x.OutsourceOrderMaterialsId == m.OutsourceOrderMaterialsId);
                    if (s != null)
                        _mapper.Map(s, m);
                    else m.IsDeleted = true;
                }

                var newMaterials = req.OutsourceOrderMaterials
                    .AsQueryable()
                    .Where(x => x.OutsourceOrderMaterialsId <= 0)
                    .ProjectTo<OutsourceOrderMaterialsEnity>(_mapper.ConfigurationProvider)
                    .ToList();
                newMaterials.ForEach(x => x.OutsourceOrderId = outsourceStepOrder.OutsourceOrderId);

                await _manufacturingDBContext.OutsourceOrderMaterials.AddRangeAsync(newMaterials);
                await _manufacturingDBContext.SaveChangesAsync();


                //update status request
                var objectIds = details.Select(x => x.ObjectId).ToList();
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
            var order = await _manufacturingDBContext.OutsourceOrder
              .FirstOrDefaultAsync(x => x.OutsourceOrderId == outsourceStepOrderId);
            if (order == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundOutsourceOrder);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                order.IsDeleted = true;

                var detail = await _manufacturingDBContext.OutsourceOrderDetail
                    .Where(x => x.OutsourceOrderId == order.OutsourceOrderId)
                    .ToListAsync();
                detail.ForEach(x => x.IsDeleted = true);

                var materials = await _manufacturingDBContext.OutsourceOrderMaterials
                    .Where(x => x.OutsourceOrderId == order.OutsourceOrderId)
                    .ToListAsync();
                materials.ForEach(x => x.IsDeleted = true);

                await _manufacturingDBContext.SaveChangesAsync();
                await UpdateOutsourceStepRequestStatus(detail.Select(x => x.ObjectId));
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, order.OutsourceOrderId, $"Loại bỏ đơn hàng gia công công đoạn {order.OutsourceOrderCode}", order.JsonSerialize());
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

        private async Task CheckMarkInvalidOutsourceStepRequest(long[] productStepLinkDataIds)
        {
            var requestIds = await _manufacturingDBContext.OutsourceStepRequestData
                .Where(r => productStepLinkDataIds.Contains(r.ProductionStepLinkDataId))
                .Select(x => x.OutsourceStepRequestId).ToListAsync();

            var outsourceStepRequests = (await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Where(x => requestIds.Contains(x.OutsourceStepRequestId) && x.IsInvalid)
                .Select(x => x.OutsourceStepRequestCode)
                .ToListAsync());

            if (outsourceStepRequests.Count > 0)
                throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"YCGC \"{String.Join(", ", outsourceStepRequests)}\" chưa xác thực với QTSX");

        }

        private async Task<bool> UpdateOutsourceStepRequestStatus(IEnumerable<long> ObjectIds) {
            var stepIds = await _manufacturingDBContext.OutsourceStepRequestData.AsNoTracking()
                .Where(x => ObjectIds.Contains(x.ProductionStepLinkDataId))
                .Select(x => x.OutsourceStepRequestId)
                .Distinct()
                .ToArrayAsync();
            return await _outsourceStepRequestService.UpdateOutsourceStepRequestStatus(stepIds);
        }

        public async Task<bool> UpdateOutsourceStepOrderStatus(long outsourceStepOrderId)
        {
            var detail = await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                .Where(x => x.OutsourceOrderId == outsourceStepOrderId).ToListAsync();
            return await UpdateOutsourceStepRequestStatus(detail.Select(x => x.ObjectId));
        }
    }
}
