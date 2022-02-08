using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service;
using VErp.Services.PurchaseOrder.Service.Voucher;

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