using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Services.Manafacturing.Model.Stages;
using VErp.Services.Manafacturing.Service.Stages;

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
        public async Task<ProductionProcessModel> GetProductionProcessByProductId([FromRoute]int productId)
        {
            return await _productionProcessService.GetProductionProcessByProductId(productId);
        }

        [HttpGet]
        [Route("{productId}/{stagesId}")]
        public async Task<ProductionStagesModel> GetProductionStagesById([FromRoute] int productId, [FromRoute] int stagesId)
        {
            return await _productionProcessService.GetProductionStagesById(productId, stagesId);
        }

        [HttpPut]
        [Route("{productId}/{stagesId}")]
        public async Task<bool> UpdateProductionStagesById([FromRoute] int productId, [FromRoute] int stagesId,[FromBody] ProductionStagesModel req)
        {
            return await _productionProcessService.UpdateProductionStagesById(productId, stagesId, req);
        }

        [HttpPost]
        [Route("{productId}")]
        public async Task<int> CreateProductionStages([FromRoute]int productId,[FromBody] ProductionStagesModel req)
        {
            return await _productionProcessService.CreateProductionStages(productId, req);
        }

        [HttpDelete]
        [Route("{productId}/{stagesId}")]
        public async Task<bool> DeleteProductionStagesById([FromRoute] int productId, [FromRoute] int stagesId)
        {
            return await _productionProcessService.DeleteProductionStagesById(productId, stagesId);
        }

        [HttpPost]
        [Route("{productId}/stagesMapping")]
        public async Task<bool> GenerateStagesMapping([FromRoute]int productId, [FromBody] List<ProductionStagesMappingModel> req)
        {
            return await _productionProcessService.GenerateStagesMapping(productId, req);
        }
    }
}
