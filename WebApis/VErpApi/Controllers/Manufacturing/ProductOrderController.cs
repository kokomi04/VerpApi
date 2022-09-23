using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionOrder.Implement;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/ProductOrder")]
    [ApiController]
    public class ProductOrderController : VErpBaseController
    {
        private readonly IProductionOrderService _productionOrderService;
        private readonly IProductionOrderMaterialsService _productionOrderMaterialsService;
        private readonly IProductionOrderMaterialSetService _productionOrderMaterialSetService;
        private readonly IValidateProductionOrderService _validateProductionOrderService;

        public ProductOrderController(IProductionOrderService productionOrderService, IProductionOrderMaterialsService productionOrderMaterialsService, IProductionOrderMaterialSetService productionOrderMaterialSetService, IValidateProductionOrderService validateProductionOrderService)
        {
            _productionOrderService = productionOrderService;
            _productionOrderMaterialsService = productionOrderMaterialsService;
            _productionOrderMaterialSetService = productionOrderMaterialSetService;
            _validateProductionOrderService = validateProductionOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<ProductionOrderInputModel> CreateProductionOrder([FromBody] ProductionOrderInputModel req)
        {
            return await _productionOrderService.CreateProductionOrder(req);
        }

        [HttpPost]
        [Route("order-product")]
        public async Task<IList<OrderProductInfo>> GetOrderProductInfo([FromBody] IList<long> productionOderIds)
        {
            return await _productionOrderService.GetOrderProductInfo(productionOderIds);
        }

        [HttpPost]
        [Route("multiple/month-plan/{monthPlanId}")]
        public async Task<int> CreateMultipleProductionOrder([FromRoute] int monthPlanId, [FromBody] ProductionOrderInputModel[] req)
        {
            return await _productionOrderService.CreateMultipleProductionOrder(monthPlanId, req);
        }

        [HttpPut]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderInputModel> UpdateProductionOrder([FromRoute] long productionOrderId, [FromBody] ProductionOrderInputModel req)
        {
            return await _productionOrderService.UpdateProductionOrder(productionOrderId, req);
        }


        [HttpPut]
        [Route("{productionOrderDetailId}/note")]
        public async Task<bool> EditNote([FromRoute] long productionOrderDetailId, [FromQuery] string note)
        {
            return await _productionOrderService.EditNote(productionOrderDetailId, note);
        }

        [HttpPut]
        [Route("update-datetime")]
        public async Task<bool> EditDate([FromBody] UpdateDatetimeModel data)
        {
            return await _productionOrderService.EditDate(data);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("Search")]
        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders(
            [FromQuery] int? monthPlanId,
            [FromQuery] int? factoryDepartmentId,
            [FromQuery] string keyword,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string orderByFieldName,
            [FromQuery] bool asc,
            [FromQuery] long fromDate,
            [FromQuery] long toDate,
            [FromQuery] bool? hasNewProductionProcessVersion,
            [FromBody] Clause filters = null)
        {
            return await _productionOrderService.GetProductionOrders(monthPlanId, factoryDepartmentId, keyword, page, size, orderByFieldName, asc, fromDate, toDate, hasNewProductionProcessVersion, filters);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetProductionOrderList")]
        public async Task<PageData<ProductOrderModelExtra>> GetProductionOrderList(
            [FromQuery] string keyword,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string orderByFieldName,
            [FromQuery] bool asc,
            [FromQuery] long fromDate,
            [FromQuery] long toDate,
            [FromBody] Clause filters = null)
        {
            return await _productionOrderService.GetProductionOrderList(keyword, page, size, orderByFieldName, asc, fromDate, toDate, filters);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetByCodes")]
        public async Task<IList<ProductionOrderListModel>> GetProductionOrders([FromBody] IList<string> productionOrderCodes)
        {
            return await _productionOrderService.GetProductionOrdersByCodes(productionOrderCodes);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetByIds")]
        public async Task<IList<ProductionOrderListModel>> GetProductionOrdersByIds([FromBody] IList<long> productionOrderIds)
        {
            return await _productionOrderService.GetProductionOrdersByIds(productionOrderIds);
        }


        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<ProductionOrderOutputModel> GetProductionOrder([FromRoute] long productionOrderId)
        {
            return await _productionOrderService.GetProductionOrder(productionOrderId);
        }


        [HttpGet]
        [Route("GetProductionHistoryByOrder")]
        public async Task<IList<ProductionOrderDetailByOrder>> GetProductionHistoryByOrder([FromQuery] IList<int> productIds, [FromQuery] IList<string> orderCodes)
        {
            return await _productionOrderService
                .GetProductionHistoryByOrder(orderCodes, productIds);
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
        [HttpPut]
        [Route("multiple")]
        public async Task<bool> UpdateMultipleProductionOrders([FromBody] ProductionOrderMultipleUpdateModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _productionOrderService.UpdateMultipleProductionOrders(data.UpdateDatas, data.ProductionOrderIds);
        }

        [HttpGet]
        [Route("{productionOrderId}/materials-sets")]
        public async Task<ProductionOrderMaterialInfo> GetProductionOrderMaterialInfo([FromRoute] int productionOrderId)
        {
            return await _productionOrderMaterialSetService.GetProductionOrderMaterialInfo(productionOrderId);
        }

        [HttpPut]
        [Route("{productionOrderId}/materials-sets")]
        public async Task<bool> UpdateProductionOrderMaterials([FromRoute] long productionOrderId, [FromBody] IList<ProductionOrderMaterialSetModel> sets)
        {
            return await _productionOrderMaterialSetService.UpdateAll(productionOrderId, sets);
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

        [HttpGet]
        [Route("capacity")]
        public async Task<ProductionCapacityModel> GetProductionCapacity([FromQuery] int monthPlanId, [FromQuery] long startDate, [FromQuery] long endDate, [FromQuery] int? assignDepartmentId)
        {
            return await _productionOrderService.GetProductionCapacity(monthPlanId, startDate, endDate, assignDepartmentId);
        }

        [HttpGet]
        [Route("Workloads")]
        public async Task<IList<ProductionStepWorkloadModel>> ListWorkLoads(long productionOrderId)
        {
            return await _productionOrderService.ListWorkLoads(productionOrderId);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("WorkloadsByProductionOrders")]
        public async Task<IList<ProductionOrderStepWorkloadModel>> ListWorkLoadsByMultipleProductionOrders([FromBody] IList<long> productionOrderIds)
        {
            return await _productionOrderService.ListWorkLoadsByMultipleProductionOrders(productionOrderIds);
        }
        

        [HttpGet]
        [Route("configuration")]
        public async Task<ProductionOrderConfigurationModel> GetProductionOrderConfiguration()
        {
            return await _productionOrderService.GetProductionOrderConfiguration();
        }

        [HttpPut]
        [Route("configuration")]
        public async Task<bool> UpdateProductionOrderConfiguration(ProductionOrderConfigurationModel model)
        {
            return await _productionOrderService.UpdateProductionOrderConfiguration(model);
        }

        [HttpPut]
        [Route("{productionOrderId}/productionProcessVersion")]
        public async Task<bool> UpdateProductionProcessVersion([FromRoute] long productionOrderId, [FromQuery] int productId)
        {
            return await _productionOrderService.UpdateProductionProcessVersion(productionOrderId, productId);
        }
    }
}
