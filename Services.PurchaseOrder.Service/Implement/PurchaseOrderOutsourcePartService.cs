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
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service.Implement
{

    public class PurchaseOrderOutsourcePartService: PurchaseOrderOutsourceAbstract, IPurchaseOrderOutsourcePartService
    {
        public PurchaseOrderOutsourcePartService(
            PurchaseOrderDBContext purchaseOrderDBContext,
            IOptions<AppSetting> appSetting,
            ILogger<PurchaseOrderOutsourcePartService> logger,
            IActivityLogService activityLogService,
            ICurrentContextService currentContext,
            IObjectGenCodeService objectGenCodeService,
            ICustomGenCodeHelperService customGenCodeHelperService,
            IMapper mapper): base(
                purchaseOrderDBContext,
                appSetting,
                logger,
                activityLogService,
                currentContext,
                objectGenCodeService,
                customGenCodeHelperService,
                mapper)
        {
        }
        public async Task<IList<RefOutsourcePartRequestModel>> GetOutsourcePartRequest()
        {
            var calculatorTotalQuantityByOutsourceStep = (from d in _purchaseOrderDBContext.PurchaseOrderDetail
                                                          join po in _purchaseOrderDBContext.PurchaseOrder on new { d.PurchaseOrderId, PurchaseOrderType = (int)EnumPurchasingOrderType.OutsourcePart } equals new { po.PurchaseOrderId, po.PurchaseOrderType }
                                                          group d by new { d.OutsourceRequestId, d.ProductId } into g
                                                          select new
                                                          {
                                                              g.Key.OutsourceRequestId,
                                                              g.Key.ProductId,
                                                              TotalQuantity = g.Sum(x => x.PrimaryQuantity)
                                                          }).ToList();
            var results = await _purchaseOrderDBContext.RefOutsourcePartRequest.ProjectTo<RefOutsourcePartRequestModel>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var r in results)
            {
                var c = calculatorTotalQuantityByOutsourceStep.FirstOrDefault(x => x.OutsourceRequestId == r.OutsourcePartRequestId && x.ProductId == r.ProductId);
                if (c != null)
                    r.QuantityProcessed = c.TotalQuantity;
            }

            return results;
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

        public async Task<IList<RefOutsourcePartRequestModel>> GetOutsourcePartRequest(long[] arrOutsourcePartId)
        {
            return (await GetOutsourcePartRequest()).Where(x => arrOutsourcePartId.Contains(x.OutsourcePartRequestId)).ToList();
        }
        
        private async Task<RefOutsourcePartRequestModel> GetOutsourcePartRequest(long outsourcePartId)
        {
            return (await GetOutsourcePartRequest()).FirstOrDefault(x => x.OutsourcePartRequestId == outsourcePartId);
        }

        protected override async Task<Enum> ValidateModelInput(long? poId, PurchaseOrderInput model)
        {
            if (!string.IsNullOrEmpty(model.PurchaseOrderCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderCode == model.PurchaseOrderCode && r.PurchaseOrderId != poId);
                if (existedItem != null) return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            }

            var notExistsOutsourcePartId = model.Details.Any(x => x.OutsourceRequestId.HasValue == false);
            if (notExistsOutsourcePartId)
                return PurchaseOrderErrorCode.NotExistsOutsourceRequestId;

            var arrOutsourcePartId = model.Details.Select(x => x.OutsourceRequestId.Value).ToArray();
            var refOutsources = await GetOutsourcePartRequest(arrOutsourcePartId);

            var isPrimaryQuanityGreaterThanQuantityRequirment = (from d in model.Details.Where(d => d.PurchaseOrderDetailId.HasValue == false)
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

                isPrimaryQuanityGreaterThanQuantityRequirment = (from d in model.Details.Where(d => d.PurchaseOrderDetailId.HasValue == true)
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