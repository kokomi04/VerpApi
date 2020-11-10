using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Commons.Enums.Manafacturing;

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
        [Route("{containerTypeId}/{containerId}")]
        public async Task<ProductionProcessInfo> GetProductionProcessByProductId([FromRoute] EnumProductionProcess.ContainerType containerTypeId, [FromRoute]int containerId)
        {
            return await _productionProcessService.GetProductionProcessByContainerId(containerTypeId, containerId);
        }

        [HttpGet]
        [Route("{containerId}/{productionStepId}")]
        public async Task<ProductionStepModel> GetProductionStepById([FromRoute] int containerId, [FromRoute] long productionStepId)
        {
            return await _productionProcessService.GetProductionStepById(containerId, productionStepId);
        }

        [HttpPut]
        [Route("{containerId}/{productionStepId}")]
        public async Task<bool> UpdateProductionStepsById([FromRoute] int containerId, [FromRoute] long productionStepId, [FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.UpdateProductionStepById(containerId, productionStepId, req);
        }

        [HttpPost]
        [Route("{containerId}")]
        public async Task<long> CreateProductionStep([FromRoute]int containerId, [FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.CreateProductionStep(containerId, req);
        }

        [HttpDelete]
        [Route("{containerId}/{productionStepId}")]
        public async Task<bool> DeleteProductionStepById([FromRoute] int containerId, [FromRoute] int productionStepId)
        {
            return await _productionProcessService.DeleteProductionStepById(containerId, productionStepId);
        }

        [HttpPost]
        [Route("productionOrder/{productionOrderId}")]
        public async Task<bool> CreateProductionProcess([FromRoute]int productionOrderId)
        {
            return await _productionProcessService.CreateProductionProcess(productionOrderId);
        }

        [HttpPut]
        [Route("productionOrder/{productionOrderId}")]
        public async Task<bool> MergeProductionProcess([FromRoute]int productionOrderId, [FromBody] IList<long> productionStepIds)
        {
            return await _productionProcessService.MergeProductionProcess(productionOrderId, productionStepIds);
        }
    }
}
