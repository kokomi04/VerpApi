using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Service;

namespace VErpApi.Controllers.PurchaseOrder.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalPurchaseOrderController : CrossServiceBaseController
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public InternalPurchaseOrderController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpPut]
        [Route("outsourcePart/{outsourcePartRequestId}/removeFromPO")]
        public async Task<bool> RemoveOutsourcePart([FromBody] long[] arrPurchaseOrderId, [FromRoute] long outsourcePartRequestId)
        {
            return await _purchaseOrderService.RemoveOutsourcePart(arrPurchaseOrderId, outsourcePartRequestId);
        }
    }
}