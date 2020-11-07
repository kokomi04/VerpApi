using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.ProductionProcess;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionProcessController : ControllerBase
    {
        private readonly IProductionProcessService _productionProcessService;

        public ProductionProcessController(IProductionProcessService productionProcessService)
        {
            _productionProcessService = productionProcessService;
        }

        [HttpGet]
        [Route("{containerId}")]
        public async Task<ProductionProcessInfo> GetProductionProcessByProductId([FromRoute]long containerId)
        {
            return await _productionProcessService.GetProductionProcessByProductId(containerId);
        }

        [HttpGet]
        [Route("{containerId}/{productionStepId}")]
        public async Task<ProductionStepModel> GetProductionStepById([FromRoute] long containerId, [FromRoute] long productionStepId)
        {
            return await _productionProcessService.GetProductionStepById(containerId, productionStepId);
        }

        [HttpPut]
        [Route("{containerId}/{productionStepId}")]
        public async Task<bool> UpdateProductionStepsById([FromRoute] long containerId, [FromRoute] long productionStepId,[FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.UpdateProductionStepById(containerId, productionStepId, req);
        }

        [HttpPost]
        [Route("{containerId}")]
        public async Task<long> CreateProductionStep([FromRoute]long containerId,[FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.CreateProductionStep(containerId, req);
        }

        [HttpDelete]
        [Route("{containerId}/{productionStepId}")]
        public async Task<bool> DeleteProductionStepById([FromRoute] int containerId, [FromRoute] int productionStepId)
        {
            return await _productionProcessService.DeleteProductionStepById(containerId, productionStepId);
        }
        
    }
}
