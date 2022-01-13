using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;
using VErp.Commons.Library;

namespace VErp.Services.PurchaseOrder.Service.Implement {

    public class PurchaseOrderOutsourcePartService: PurchaseOrderOutsourceAbstract, IPurchaseOrderOutsourcePartService
    {
        public PurchaseOrderOutsourcePartService(
            PurchaseOrderDBContext purchaseOrderDBContext,
            IOptions<AppSetting> appSetting,
            ILogger<PurchaseOrderOutsourcePartService> logger,
            IActivityLogService activityLogService,
            ICurrentContextService currentContext,
            ICustomGenCodeHelperService customGenCodeHelperService,
            IManufacturingHelperService manufacturingHelperService,
            IMapper mapper): base(
                purchaseOrderDBContext,
                appSetting,
                logger,
                activityLogService,
                currentContext,
                customGenCodeHelperService,
                manufacturingHelperService,
                mapper)
        {
        }
        public async Task<IList<RefOutsourcePartRequestModel>> GetOutsourcePartRequest(long[] outsourcePartRequestId, string productionOrderCode, int? productId)
        {
            var queryRefOutsourcePart = _purchaseOrderDBContext.RefOutsourcePartRequest.AsQueryable();

            if(outsourcePartRequestId != null && outsourcePartRequestId.Length > 0 )
                queryRefOutsourcePart = queryRefOutsourcePart.Where(x => outsourcePartRequestId.Contains(x.OutsourcePartRequestId));

            var calculatorTotalQuantityByOutsourcePart = (from d in _purchaseOrderDBContext.PurchaseOrderDetail
                                                          join po in _purchaseOrderDBContext.PurchaseOrder on new { d.PurchaseOrderId, PurchaseOrderType = (int)EnumPurchasingOrderType.OutsourcePart } equals new { po.PurchaseOrderId, po.PurchaseOrderType }
                                                          group d by new { d.OutsourceRequestId, d.ProductId } into g
                                                          select new
                                                          {
                                                              g.Key.OutsourceRequestId,
                                                              g.Key.ProductId,
                                                              TotalQuantity = (decimal?)g.Sum(x => x.PrimaryQuantity)
                                                          });

            var query = (from o in queryRefOutsourcePart
                                 join c in calculatorTotalQuantityByOutsourcePart on new { o.OutsourcePartRequestId, o.ProductId } equals new { OutsourcePartRequestId = c.OutsourceRequestId.GetValueOrDefault(), c.ProductId } into gc
                                 from c in gc.DefaultIfEmpty()
                                 where c.TotalQuantity.HasValue == false && (o.Quantity - c.TotalQuantity.GetValueOrDefault()) > 0
                                 select new RefOutsourcePartRequestModel
                                 {
                                     ProductId = o.ProductId,
                                     ProductionOrderCode = o.ProductionOrderCode,
                                     ProductionOrderId = o.ProductionOrderId,
                                     Quantity = o.Quantity,
                                     QuantityProcessed = c.TotalQuantity.GetValueOrDefault(),
                                     OutsourcePartRequestCode = o.OutsourcePartRequestCode,
                                     OutsourcePartRequestDetailFinishDate = o.OutsourcePartRequestDetailFinishDate.GetUnix(),
                                     OutsourcePartRequestId = o.OutsourcePartRequestId,
                                     ProductionOrderDetailId = o.ProductionOrderDetailId,
                                     RootProductId = o.RootProductId
                                 });

            if (!string.IsNullOrWhiteSpace(productionOrderCode))
                query = query.Where(x => x.ProductionOrderCode == productionOrderCode);
            if (productId.HasValue)
                query = query.Where(x => x.ProductId == productId.GetValueOrDefault());

            var results = await query.ToListAsync();
            return results.OrderByDescending(x => x.OutsourcePartRequestId).ToList();
        }
        
        public async Task<long> CreatePurchaseOrderOutsourcePart(PurchaseOrderInput model)
        {
            return await CreatePurchaseOrderOutsource(model, EnumPurchasingOrderType.OutsourcePart);
        }

        public async Task<bool> UpdatePurchaseOrderOutsourcePart(long purchaseOrderId, PurchaseOrderInput model)
        {
            return await UpdatePurchaseOrderOutsource(purchaseOrderId, model);
        }

        public async Task<bool> DeletePurchaseOrderOutsourcePart(long purchaseOrderId)
        {
            return await DeletePurchaseOrderOutsource(purchaseOrderId);
        }

        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourcePart(long purchaseOrderId){
            return await GetPurchaseOrderOutsource(purchaseOrderId);
        }

        public async Task<bool> UpdateStatusForOutsourceRequestInPurcharOrder(long purchaseOrderId)
        {
            var outsourceRequestId = await GetAllOutsourceRequestIdInPurchaseOrder(purchaseOrderId);

            return await _manufacturingHelperService.UpdateOutsourcePartRequestStatus(outsourceRequestId);
        }
        
        private async Task<RefOutsourcePartRequestModel> GetOutsourcePartRequest(long outsourcePartId)
        {
            return (await GetOutsourcePartRequest(new [] { outsourcePartId }, "", null)).FirstOrDefault();
        }

        protected override async Task<Enum> ValidateModelInput(long? poId, PurchaseOrderInput model)
        {
            if (!string.IsNullOrEmpty(model.PurchaseOrderCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderCode == model.PurchaseOrderCode && r.PurchaseOrderId != poId);
                if (existedItem != null) return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            }

            // var notExistsOutsourcePartId = model.Details.Any(x => x.OutsourceRequestId.HasValue == false);
            // if (notExistsOutsourcePartId)
            //     return PurchaseOrderErrorCode.NotExistsOutsourceRequestId;

            var arrOutsourcePartId = model.Details.Where(x => x.OutsourceRequestId.HasValue == true).Select(x => x.OutsourceRequestId.Value).ToArray();
            var refOutsources = await GetOutsourcePartRequest(arrOutsourcePartId, string.Empty, null);

            var isPrimaryQuanityGreaterThanQuantityRequirment = (from d in model.Details.Where(d => d.PurchaseOrderDetailId.HasValue == false && d.OutsourceRequestId.HasValue == true)
                                                                 join r in refOutsources on new { OutsourceRequestId = d.OutsourceRequestId.Value, d.ProductId } equals new { OutsourceRequestId = r.OutsourcePartRequestId, r.ProductId }
                                                                 select new
                                                                 {
                                                                     d.PrimaryQuantity,
                                                                     QuantityRequirement = r.Quantity - r.QuantityProcessed
                                                                 }).Any(x => x.PrimaryQuantity > x.QuantityRequirement);

            if (poId.HasValue)
            {
                var arrDetailId = model.Details.Where(d => d.PurchaseOrderDetailId > 0).Select(d => d.PurchaseOrderDetailId).Distinct().ToArray();
                var details = await _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking().Where(d => arrDetailId.Contains(d.PurchaseOrderDetailId)).ToListAsync();

                isPrimaryQuanityGreaterThanQuantityRequirment = (from d in model.Details.Where(d => d.PurchaseOrderDetailId.HasValue == true && d.OutsourceRequestId.HasValue == true)
                                                                 join o in details on d.PurchaseOrderDetailId equals o.PurchaseOrderDetailId
                                                                 join r in refOutsources on new { OutsourceRequestId = d.OutsourceRequestId.Value, d.ProductId } equals new { OutsourceRequestId = r.OutsourcePartRequestId, r.ProductId }
                                                                 select new
                                                                 {
                                                                     d.PrimaryQuantity,
                                                                     QuantityRequirement = r.Quantity - r.QuantityProcessed + o.PrimaryQuantity
                                                                 }).Any(x => x.PrimaryQuantity > x.QuantityRequirement);
            }

            if (isPrimaryQuanityGreaterThanQuantityRequirment)
                return PurchaseOrderErrorCode.PrimaryQuanityGreaterThanQuantityRequirment;

            return GeneralCode.Success;
        }

    }
}