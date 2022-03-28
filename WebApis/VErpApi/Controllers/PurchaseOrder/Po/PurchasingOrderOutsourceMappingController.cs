using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Services.PurchaseOrder.Service;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchasingOrder/OutsourceMapping")]
    public class PurchasingOrderOutsourceMappingController : VErpBaseController
    {

        private readonly IPurchaseOrderOutsourceMappingService _purchaseOrderService;

        public PurchasingOrderOutsourceMappingController(IPurchaseOrderOutsourceMappingService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddPurchaseOrderOutsourceMapping([FromBody] PurchaseOrderOutsourceMappingModel model)
        {
            return await _purchaseOrderService.AddPurchaseOrderOutsourceMapping(model);
        }

        [HttpDelete]
        [Route("{purchaseOrderOutsourceMappingId}")]
        public async Task<bool> DeletePurchaseOrderOutsourceMapping([FromRoute] long purchaseOrderOutsourceMappingId)
        {
            return await _purchaseOrderService.DeletePurchaseOrderOutsourceMapping(purchaseOrderOutsourceMappingId);
        }

        [HttpGet]
        [Route("byProductionOrderCode")]
        public async Task<IList<PurchaseOrderOutsourceMappingModel>> GetAllByProductionOrderCode([FromQuery] string productionOrderCode)
        {
            return await _purchaseOrderService.GetAllByProductionOrderCode(productionOrderCode);
        }

        [HttpGet]
        [Route("byPurchaseOrderDetailId")]
        public async Task<IList<PurchaseOrderOutsourceMappingModel>> GetAllByPurchaseOrderDetailId([FromQuery] long purchaseOrderDetailId)
        {
            return await _purchaseOrderService.GetAllByPurchaseOrderId(purchaseOrderDetailId);
        }

        [HttpPut]
        [Route("{purchaseOrderOutsourceMappingId}")]
        public async Task<bool> UpdatePurchaseOrderOutsourceMapping([FromBody] PurchaseOrderOutsourceMappingModel model, [FromRoute] long purchaseOrderOutsourceMappingId)
        {
            return await _purchaseOrderService.UpdatePurchaseOrderOutsourceMapping(model, purchaseOrderOutsourceMappingId);
        }
    }
}
