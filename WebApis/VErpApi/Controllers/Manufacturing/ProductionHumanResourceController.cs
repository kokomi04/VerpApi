using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionHumanResourceController : VErpBaseController
    {
        private readonly IProductionHumanResourceService _productionHumanResourceService;

        public ProductionHumanResourceController(IProductionHumanResourceService productionHumanResourceService)
        {
            _productionHumanResourceService = productionHumanResourceService;
        }

        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<IList<ProductionHumanResourceModel>> GetProductionHumanResources([FromRoute] long productionOrderId)
        {
            return await _productionHumanResourceService.GetProductionHumanResources(productionOrderId);
        }

        [HttpGet]
        [Route("department/{departmentId}")]
        public async Task<IList<ProductionHumanResourceModel>> GetProductionHumanResourceByDepartment([FromRoute] int departmentId, [FromQuery]long startDate, [FromQuery] long endDate)
        {
            return await _productionHumanResourceService.GetProductionHumanResourceByDepartment(departmentId, startDate, endDate);
        }

        [HttpGet]
        [Route("department/{departmentId}/productionOrderInfo")]
        public async Task<IList<UnFinishProductionInfo>> GetUnFinishProductionInfo([FromRoute] int departmentId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionHumanResourceService.GetUnFinishProductionInfo(departmentId, startDate, endDate);
        }


        [HttpPost]
        [Route("{productionOrderId}")]
        public async Task<ProductionHumanResourceModel> CreateProductionHumanResource([FromRoute] long productionOrderId, [FromBody] ProductionHumanResourceInputModel data)
        {
            return await _productionHumanResourceService.CreateProductionHumanResource(productionOrderId, data);
        }

        [HttpPost]
        [Route("multiple/{productionOrderId}")]
        public async Task<IList<ProductionHumanResourceModel>> CreateMultipleProductionHumanResource([FromRoute] long productionOrderId, [FromBody] IList<ProductionHumanResourceInputModel> data)
        {
            return await _productionHumanResourceService.CreateMultipleProductionHumanResource(productionOrderId, data);
        }

        [HttpPost]
        [Route("multiple/department/{departmentId}")]
        public async Task<IList<ProductionHumanResourceModel>> CreateMultipleProductionHumanResource([FromRoute] int departmentId, [FromQuery] long startDate, [FromQuery] long endDate, [FromBody] IList<ProductionHumanResourceInputModel> data)
        {
            return await _productionHumanResourceService.CreateMultipleProductionHumanResourceByDepartment(departmentId, startDate, endDate, data);
        }

        [HttpDelete]
        [Route("{productionHumanResourceId}")]
        public async Task<bool> DeleteProductionHumanResource([FromRoute] long productionHumanResourceId)
        {
            return await _productionHumanResourceService.DeleteProductionHumanResource(productionHumanResourceId);
        }
    }
}
