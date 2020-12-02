using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/ProductOrder")]
    [ApiController]
    public class ProductOrderController : VErpBaseController
    {
        private readonly IProductionOrderService _productionOrderService;

        public ProductOrderController(IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<ProductionOrderInputModel> CreateProductionOrder([FromBody] ProductionOrderInputModel req)
        {
            return await _productionOrderService.CreateProductionOrder(req);
        }

        [HttpPut]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderInputModel> UpdateProductionOrder([FromRoute] int productionOrderId, [FromBody] ProductionOrderInputModel req)
        {
            return await _productionOrderService.UpdateProductionOrder(productionOrderId, req);
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("Search")]
        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _productionOrderService.GetProductionOrders(keyword, page, size, orderByFieldName, asc, filters);
        }

        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderOutputModel> GetProductionOrder([FromRoute] int productionOrderId)
        {
            return await _productionOrderService.GetProductionOrder(productionOrderId);
        }

        [HttpGet]
        [Route("order/{orderId}")]
        public async Task<IList<ProductionOrderExtraInfo>> GetProductionOrderExtraInfo([FromRoute] int orderId)
        {
            return await _productionOrderService.GetProductionOrderExtraInfo(orderId);
        }

        [HttpDelete]
        [Route("{productionOrderId}")]
        public async Task<bool> DeleteProductionOrder([FromRoute] int productionOrderId)
        {
            return await _productionOrderService.DeleteProductionOrder(productionOrderId);
        }

        [HttpGet]
        [Route("detail/{productionOrderDetailId}")]
        public async Task<ProductionOrderDetailOutputModel> GetProductionOrderDetail([FromRoute] long? productionOrderDetailId)
        {
            return await _productionOrderService.GetProductionOrderDetail(productionOrderDetailId);
        }

    }
}
