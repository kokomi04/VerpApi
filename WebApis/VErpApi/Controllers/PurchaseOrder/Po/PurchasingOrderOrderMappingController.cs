using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Service;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchasingOrder/OrderMapping")]
    public class PurchasingOrderOrderMappingController : VErpBaseController
    {

        private readonly IPurchaseOrderOrderMappingService _purchaseOrderService;

        public PurchasingOrderOrderMappingController(IPurchaseOrderOrderMappingService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddPurchaseOrderOrderMapping([FromBody] PurchaseOrderOrderMappingModel model)
        {
            return await _purchaseOrderService.AddPurchaseOrderOrderMapping(model);
        }

        [HttpDelete]
        [Route("{purchaseOrderOrderMappingId}")]
        public async Task<bool> DeletePurchaseOrderOrderMapping([FromRoute] long purchaseOrderOrderMappingId)
        {
            return await _purchaseOrderService.DeletePurchaseOrderOrderMapping(purchaseOrderOrderMappingId);
        }

        [HttpGet]
        [Route("byPurchaseOrderDetailId")]
        public async Task<IList<PurchaseOrderOrderMappingModel>> GetAllByPurchaseOrderDetailId([FromQuery] long purchaseOrderDetailId)
        {
            return await _purchaseOrderService.GetAllByPurchaseOrderDetailId(purchaseOrderDetailId);
        }

        [HttpPut]
        [Route("{purchaseOrderOrderMappingId}")]
        public async Task<bool> UpdatePurchaseOrderOrderMapping([FromBody] PurchaseOrderOrderMappingModel model, [FromRoute] long purchaseOrderOrderMappingId)
        {
            return await _purchaseOrderService.UpdatePurchaseOrderOrderMapping(model, purchaseOrderOrderMappingId);
        }
    }
}
