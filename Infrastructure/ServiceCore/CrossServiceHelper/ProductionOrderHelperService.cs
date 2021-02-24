using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductionOrderHelperService
    {
        Task<bool> UpdateProductionOrderStatus(long productionOrderId, EnumProductionStatus status);
        Task<bool> UpdateManualProductionOrderInventoryRequirements(long? inventoryRequirementId);
        Task<bool> UpdateManualProductionOrderPurchasingRequirements(long? purchasingRequestId);
    }
    public class ProductionOrderHelperService : IProductionOrderHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductionOrderHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> UpdateProductionOrderStatus(long productionOrderId, EnumProductionStatus status)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionOrder/{productionOrderId}/status", new { ProductionOrderStatus = status });
        }

       
        public async Task<bool> UpdateManualProductionOrderInventoryRequirements(long? inventoryRequirementId)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionOrder/requirements/inventory?inventoryRequirementId={inventoryRequirementId}", null);
        }

        public async Task<bool> UpdateManualProductionOrderPurchasingRequirements(long? purchasingRequestId)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionOrder/requirements/purchasing?purchasingRequestId={purchasingRequestId}", null);
        }
    }
}
