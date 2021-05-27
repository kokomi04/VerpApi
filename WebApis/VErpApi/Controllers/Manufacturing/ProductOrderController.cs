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
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/ProductOrder")]
    [ApiController]
    public class ProductOrderController : VErpBaseController
    {
        private readonly IProductionOrderService _productionOrderService;
        private readonly IProductionOrderMaterialsService _productionOrderMaterialsService;
        private readonly IValidateProductionOrderService _validateProductionOrderService;

        public ProductOrderController(IProductionOrderService productionOrderService, IProductionOrderMaterialsService productionOrderMaterialsService, IValidateProductionOrderService validateProductionOrderService)
        {
            _productionOrderService = productionOrderService;
            _productionOrderMaterialsService = productionOrderMaterialsService;
            _validateProductionOrderService = validateProductionOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<ProductionOrderInputModel> CreateProductionOrder([FromBody] ProductionOrderInputModel req)
        {
            return await _productionOrderService.CreateProductionOrder(req);
        }

        [HttpPut]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderInputModel> UpdateProductionOrder([FromRoute] long productionOrderId, [FromBody] ProductionOrderInputModel req)
        {
            return await _productionOrderService.UpdateProductionOrder(productionOrderId, req);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("Search")]
        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _productionOrderService.GetProductionOrders(keyword, page, size, orderByFieldName, asc, filters);
        }

        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderOutputModel> GetProductionOrder([FromRoute] long productionOrderId)
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
        public async Task<bool> DeleteProductionOrder([FromRoute] long productionOrderId)
        {
            return await _productionOrderService.DeleteProductionOrder(productionOrderId);
        }

        [HttpGet]
        [Route("detail/{productionOrderDetailId}")]
        public async Task<ProductionOrderDetailOutputModel> GetProductionOrderDetail([FromRoute] long? productionOrderDetailId)
        {
            return await _productionOrderService.GetProductionOrderDetail(productionOrderDetailId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ProductOrderModel>> GetProductionOrders()
        {
            return await _productionOrderService.GetProductionOrders();
        }

        [HttpPut]
        [Route("{productionOrderId}/status")]
        public async Task<bool> UpdateManualProductionOrderStatus([FromRoute] long productionOrderId, [FromBody] ProductionOrderStatusDataModel status)
        {
            return await _productionOrderService.UpdateManualProductionOrderStatus(productionOrderId, status);
        }

        [HttpGet]
        [Route("{productionOrderId}/materials-calc")]
        public async Task<ProductionOrderMaterialsModel> GetProductionOrderMaterials([FromRoute] int productionOrderId)
        {
            return await _productionOrderMaterialsService.GetProductionOrderMaterialsCalc(productionOrderId);
        }

        [HttpPut]
        [Route("{productionOrderId}/materials")]
        public async Task<bool> UpdateProductionOrderMaterials([FromRoute] long productionOrderId, [FromBody] IList<ProductionOrderMaterialsInput> materials)
        {
            return await _productionOrderMaterialsService.UpdateProductionOrderMaterials(productionOrderId, materials);
        }

        [HttpGet]
        [Route("{productionOrderId}/materials")]
        public async Task<IList<ProductionOrderMaterialsOutput>> GetProductionOrderMaterials(long productionOrderId)
        {
            return await _productionOrderMaterialsService.GetProductionOrderMaterials(productionOrderId);
        }

        [HttpGet]
        [Route("{productionOrderId}/warnings")]
        public async Task<IList<string>> GetWarnings([FromRoute] int productionOrderId)
        {
            return await _validateProductionOrderService.ValidateProductionOrder(productionOrderId);
        }

    }
}
