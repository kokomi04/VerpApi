using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionOrder;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductionOrderController : CrossServiceBaseController
    {
        private readonly IProductionOrderService _productionOrderService;
        public InternalProductionOrderController(IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
        }

        [HttpPut]
        [Route("{productionOrderId}/status")]
        public async Task<bool> UpdateProductionOrderStatus([FromRoute] long productionOrderId, [FromBody] ProductionOrderStatusModel status)
        {
            return await _productionOrderService.UpdateProductionOrderStatus(productionOrderId, status);
        }

        [HttpPut]
        [Route("requirements/inventory")]
        public async Task<bool> DeleteManualProductionOrderInventoryRequirements([FromQuery] long? inventoryRequirementId)
        {
            return await _productionOrderService.DeleteManualProductionOrderInventoryRequirements(inventoryRequirementId);
        }

        [HttpPut]
        [Route("requirements/purchasing")]
        public async Task<bool> DeleteManualProductionOrderPurchasingRequirements([FromQuery] long? purchasingRequestId)
        {
            return await _productionOrderService.DeleteManualProductionOrderPurchasingRequirements(purchasingRequestId);
        }
    }
}