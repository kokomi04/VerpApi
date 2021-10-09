using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Library;
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

namespace VErp.Services.PurchaseOrder.Service.Implement
{

    public class PurchaseOrderOutsourceStepService : PurchaseOrderOutsourceAbstract, IPurchaseOrderOutsourceStepService
    {
        public PurchaseOrderOutsourceStepService(
            PurchaseOrderDBContext purchaseOrderDBContext,
            IOptions<AppSetting> appSetting,
            ILogger<PurchaseOrderOutsourcePartService> logger,
            IActivityLogService activityLogService,
            ICurrentContextService currentContext,
            ICustomGenCodeHelperService customGenCodeHelperService,
            IManufacturingHelperService manufacturingHelperService,
            IMapper mapper) : base(
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

        public async Task<IList<RefOutsourceStepRequestModel>> GetOutsourceStepRequest(long[] arrOutsourceStepId)
        {
            
            var queryRefOutsourceStep = _purchaseOrderDBContext.RefOutsourceStepRequest.AsQueryable();

            if (arrOutsourceStepId != null && arrOutsourceStepId.Length > 0)
                queryRefOutsourceStep = queryRefOutsourceStep.Where(x => arrOutsourceStepId.Contains(x.OutsourceStepRequestId));

            var calculatorTotalQuantityByOutsourceStep = (from d in _purchaseOrderDBContext.PurchaseOrderDetail
                                                          join po in _purchaseOrderDBContext.PurchaseOrder on new { d.PurchaseOrderId, PurchaseOrderType = (int)EnumPurchasingOrderType.OutsourceStep } equals new { po.PurchaseOrderId, po.PurchaseOrderType }
                                                          group d by new { d.OutsourceRequestId, d.ProductionStepLinkDataId } into g
                                                          select new 
                                                          {
                                                              g.Key.OutsourceRequestId,
                                                              g.Key.ProductionStepLinkDataId,
                                                              TotalQuantity = (decimal?)g.Sum(x => x.PrimaryQuantity)
                                                          });
            var results = await (from o in queryRefOutsourceStep
                                 join c in calculatorTotalQuantityByOutsourceStep on new { o.OutsourceStepRequestId, o.ProductionStepLinkDataId } equals new { OutsourceStepRequestId = c.OutsourceRequestId.GetValueOrDefault(), ProductionStepLinkDataId = c.ProductionStepLinkDataId.GetValueOrDefault() } into gc
                                 from c in gc.DefaultIfEmpty()
                                 where c.TotalQuantity.HasValue == false && (o.Quantity - c.TotalQuantity.GetValueOrDefault()) > 0
                                 select new RefOutsourceStepRequestModel
                                 {
                                     IsImportant = o.IsImportant,
                                     OutsourceStepRequestCode = o.OutsourceStepRequestCode,
                                     OutsourceStepRequestFinishDate = o.OutsourceStepRequestFinishDate.GetUnix(),
                                     OutsourceStepRequestId = o.OutsourceStepRequestId,
                                     ProductId = o.ProductId,
                                     ProductionOrderCode = o.ProductionOrderCode,
                                     ProductionOrderId = o.ProductionOrderId,
                                     ProductionStepId = o.ProductionStepId,
                                     ProductionStepLinkDataId = o.ProductionStepLinkDataId,
                                     ProductionStepLinkDataRoleTypeId = o.ProductionStepLinkDataRoleTypeId,
                                     Quantity = o.Quantity,
                                     StepId = o.StepId,
                                     QuantityProcessed = c.TotalQuantity.GetValueOrDefault()
                                 }).ToListAsync();

            return results.OrderByDescending(x => x.OutsourceStepRequestId).ToList();
        }

        public async Task<long> CreatePurchaseOrderOutsourceStep(PurchaseOrderInput model)
        {
            return await CreatePurchaseOrderOutsource(model, EnumPurchasingOrderType.OutsourceStep);
        }

        public async Task<bool> UpdatePurchaseOrderOutsourceStep(long purchaseOrderId, PurchaseOrderInput model)
        {
            return await UpdatePurchaseOrderOutsource(purchaseOrderId, model);
        }

        public async Task<bool> DeletePurchaseOrderOutsourceStep(long purchaseOrderId)
        {
            return await DeletePurchaseOrderOutsource(purchaseOrderId);
        }

        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourceStep(long purchaseOrderId)
        {
            return await GetPurchaseOrderOutsource(purchaseOrderId);
        }

        public async Task<bool> UpdateStatusForOutsourceRequestInPurcharOrder(long purchaseOrderId)
        {
            var outsourceRequestId = await GetAllOutsourceRequestIdInPurchaseOrder(purchaseOrderId);

            return await _manufacturingHelperService.UpdateOutsourceStepRequestStatus(outsourceRequestId);
        }


        private async Task<RefOutsourceStepRequestModel> GetOutsourceStepRequest(long outsourceStepRequestId)
        {
            return (await GetOutsourceStepRequest(new[] { outsourceStepRequestId })).FirstOrDefault();
        }

        protected override async Task<Enum> ValidateModelInput(long? poId, PurchaseOrderInput model)
        {
            if (!string.IsNullOrEmpty(model.PurchaseOrderCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderCode == model.PurchaseOrderCode && r.PurchaseOrderId != poId);
                if (existedItem != null) return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            }

            var notExistsOutsourceStepId = model.Details.Any(x => x.OutsourceRequestId.HasValue == false || x.ProductionStepLinkDataId.HasValue == false);
            if (notExistsOutsourceStepId)
                return PurchaseOrderErrorCode.NotExistsOutsourceRequestId;

            var arrOutsourceStepId = model.Details.Select(x => x.OutsourceRequestId.Value).ToArray();
            var refOutsources = await GetOutsourceStepRequest(arrOutsourceStepId);

            var isPrimaryQuanityGreaterThanQuantityRequirment = (from d in model.Details.Where(d => d.PurchaseOrderDetailId.HasValue == false)
                                                                 join r in refOutsources on new { OutsourceRequestId = d.OutsourceRequestId.Value, ProductionStepLinkDataId = d.ProductionStepLinkDataId.Value } equals new { OutsourceRequestId = r.OutsourceStepRequestId, r.ProductionStepLinkDataId }
                                                                 select new
                                                                 {
                                                                     d.PrimaryQuantity,
                                                                     QuantityRequirement = r.Quantity - r.QuantityProcessed
                                                                 }).Any(x => x.PrimaryQuantity > x.QuantityRequirement);

            if (poId.HasValue)
            {
                var arrDetailId = model.Details.Where(d => d.PurchaseOrderDetailId > 0).Select(d => d.PurchaseOrderDetailId).Distinct().ToArray();
                var details = await _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking().Where(d => arrDetailId.Contains(d.PurchaseOrderDetailId)).ToListAsync();

                isPrimaryQuanityGreaterThanQuantityRequirment = (from d in model.Details.Where(d => d.PurchaseOrderDetailId.HasValue == true)
                                                                 join o in details on d.PurchaseOrderDetailId equals o.PurchaseOrderDetailId
                                                                 join r in refOutsources on new { OutsourceRequestId = d.OutsourceRequestId.Value, ProductionStepLinkDataId = d.ProductionStepLinkDataId.Value } equals new { OutsourceRequestId = r.OutsourceStepRequestId, r.ProductionStepLinkDataId }
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