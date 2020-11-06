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
        [Route("{productId}")]
        public async Task<ProductionProcessInfo> GetProductionProcessByProductId([FromRoute]int productId)
        {
            return await _productionProcessService.GetProductionProcessByProductId(productId);
        }

        [HttpGet]
        [Route("{productId}/{productionStepId}")]
        public async Task<ProductionStepModel> GetProductionStagesById([FromRoute] int productId, [FromRoute] int productionStepId)
        {
            return await _productionProcessService.GetProductionStepById(productId, productionStepId);
        }

        [HttpPut]
        [Route("{productId}/{productionStepId}")]
        public async Task<bool> UpdateProductionStagesById([FromRoute] int productId, [FromRoute] int productionStepId,[FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.UpdateProductionStagesById(productId, productionStepId, req);
        }

        [HttpPost]
        [Route("{productId}")]
        public async Task<int> CreateProductionStages([FromRoute]int productId,[FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.CreateProductionStep(productId, req);
        }

        [HttpDelete]
        [Route("{productId}/{productionStepId}")]
        public async Task<bool> DeleteProductionStagesById([FromRoute] int productId, [FromRoute] int productionStepId)
        {
            return await _productionProcessService.DeleteProductionStepById(productId, productionStepId);
        }

        [HttpPost]
        [Route("{productId}/stagesMapping")]
        public async Task<bool> GenerateStagesMapping([FromRoute]int productId, [FromBody] List<ProductionStepLinkModel> req)
        {
            return await _productionProcessService.GenerateProductionStepMapping(productId, req);
        }
    }
}
