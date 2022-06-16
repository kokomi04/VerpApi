using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionHandover;

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
        public async Task<IList<ProductionHumanResourceModel>> GetByProductionOrder([FromRoute] long productionOrderId)
        {
            return await _productionHumanResourceService.GetByProductionOrder(productionOrderId);
        }

        [HttpGet]
        [Route("department/{departmentId}")]
        public async Task<IList<ProductionHumanResourceModel>> GetByDepartment([FromRoute] int departmentId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionHumanResourceService.GetByDepartment(departmentId, startDate, endDate);
        }

        [HttpGet]
        [Route("department/{departmentId}/productionOrderInfo")]
        public async Task<IList<UnFinishProductionInfo>> GetUnFinishProductionInfo([FromRoute] int departmentId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionHumanResourceService.GetUnFinishProductionInfo(departmentId, startDate, endDate);
        }


        [HttpPost]
        [Route("{productionOrderId}")]
        public async Task<ProductionHumanResourceModel> Create([FromRoute] long productionOrderId, [FromBody] ProductionHumanResourceInputModel data)
        {
            return await _productionHumanResourceService.Create(productionOrderId, data);
        }

        [HttpPut]
        [Route("{productionOrderId}/update/{productionHumanResourceId}")]
        public async Task<ProductionHumanResourceModel> Update([FromRoute] long productionOrderId, [FromRoute] long productionHumanResourceId, [FromBody] ProductionHumanResourceInputModel data)
        {
            return await _productionHumanResourceService.Update(productionOrderId, productionHumanResourceId, data);
        }

        [HttpPost]
        [Route("multiple/{productionOrderId}")]
        public async Task<IList<ProductionHumanResourceModel>> CreateMultiple([FromRoute] long productionOrderId, [FromBody] IList<ProductionHumanResourceInputModel> data)
        {
            return await _productionHumanResourceService.CreateMultiple(productionOrderId, data);
        }

        [HttpPost]
        [Route("multiple/department/{departmentId}")]
        public async Task<IList<ProductionHumanResourceModel>> CreateMultipleByDepartment([FromRoute] int departmentId, [FromQuery] long startDate, [FromQuery] long endDate, [FromBody] IList<ProductionHumanResourceInputModel> data)
        {
            return await _productionHumanResourceService.CreateMultipleByDepartment(departmentId, startDate, endDate, data);
        }

        [HttpDelete]
        [Route("{productionHumanResourceId}")]
        public async Task<bool> Delete([FromRoute] long productionHumanResourceId)
        {
            return await _productionHumanResourceService.Delete(productionHumanResourceId);
        }
    }
}
