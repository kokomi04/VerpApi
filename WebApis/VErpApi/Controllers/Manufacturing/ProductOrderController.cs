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
namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/ProductOrder")]
    [ApiController]
    public class ProductOrderController : ControllerBase
    {
        private readonly IProductionOrderService _productionOrderService;

        public ProductOrderController(IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<ProductionOrderModel> CreateProductionOrder([FromBody] ProductionOrderModel req)
        {
            return await _productionOrderService.CreateProductionOrder(req);
        }

        [HttpPut]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderModel> UpdateProductionOrder([FromRoute] int productionOrderId, [FromBody] ProductionOrderModel req)
        {
            return await _productionOrderService.UpdateProductionOrder(productionOrderId, req);
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("Search")]
        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromBody] Clause filters = null)
        {
            return await _productionOrderService.GetProductionOrders(keyword, page, size, filters);
        }

        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderModel> GetProductionOrder([FromRoute] int productionOrderId)
        {
            return await _productionOrderService.GetProductionOrder(productionOrderId);
        }

        [HttpDelete]
        [Route("{productionOrderId}")]
        public async Task<bool> DeleteProductionOrder([FromRoute] int productionOrderId)
        {
            return await _productionOrderService.DeleteProductionOrder(productionOrderId);
        }
    }
}
