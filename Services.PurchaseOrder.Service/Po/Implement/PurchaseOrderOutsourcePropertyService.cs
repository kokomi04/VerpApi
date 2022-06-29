using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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

    public class PurchaseOrderOutsourcePropertyService : PurchaseOrderOutsourceAbstract, IPurchaseOrderOutsourcePropertyService
    {
        public PurchaseOrderOutsourcePropertyService(
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

        public async Task<long> CreatePurchaseOrderOutsourceProperty(PurchaseOrderInput model)
        {
            return await CreatePurchaseOrderOutsource(model, EnumPurchasingOrderType.OutsourceProperty);
        }

        public async Task<bool> UpdatePurchaseOrderOutsourceProperty(long purchaseOrderId, PurchaseOrderInput model)
        {
            return await UpdatePurchaseOrderOutsource(purchaseOrderId, model);
        }

        public async Task<bool> DeletePurchaseOrderOutsourceProperty(long purchaseOrderId)
        {
            return await DeletePurchaseOrderOutsource(purchaseOrderId);
        }

        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourceProperty(long purchaseOrderId)
        {
            return await GetPurchaseOrderOutsource(purchaseOrderId);
        }

        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsourcePropertyByPropertyCalcId(long propertyCalcId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(o => o.PropertyCalcId == propertyCalcId);
            if (info == null)
            {
                throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);
            }

            return await GetPurchaseOrderOutsource(info.PurchaseOrderId);
        }

        protected override async Task<Enum> ValidateModelInput(long? poId, PurchaseOrderInput model)
        {
            if (!string.IsNullOrEmpty(model.PurchaseOrderCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderCode == model.PurchaseOrderCode && r.PurchaseOrderId != poId);
                if (existedItem != null) return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            }

            return GeneralCode.Success;
        }

    }
}