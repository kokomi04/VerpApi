using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.ProductionProcess;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/ProductionProcess/productionStepCollection")]
    [ApiController]
    public class ProductionStepCollectionController: VErpBaseController
    {
        private readonly IProductionStepCollectionService _productionStepCollectionService;

        public ProductionStepCollectionController(IProductionStepCollectionService productionStepCollectionService)
        {
            _productionStepCollectionService = productionStepCollectionService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddProductionStepCollection([FromBody] ProductionStepCollectionModel model)
        {
            return await _productionStepCollectionService.AddProductionStepCollection(model);
        }

        [HttpPut]
        [Route("{productionStepCollectionId}")]
        public async Task<bool> UpdateProductionStepCollection([FromRoute] long productionStepCollectionId, [FromBody] ProductionStepCollectionModel model)
        {
            return await _productionStepCollectionService.UpdateProductionStepCollection(productionStepCollectionId, model);
        }

        [HttpDelete]
        [Route("{productionStepCollectionId}")]
        public async Task<bool> DeleteProductionStepCollection([FromRoute] long productionStepCollectionId)
        {
            return await _productionStepCollectionService.DeleteProductionStepCollection(productionStepCollectionId);
        }

        [HttpGet]
        [Route("{productionStepCollectionId}")]
        public async Task<ProductionStepCollectionModel> GetProductionStepCollection([FromRoute] long productionStepCollectionId)
        {
            return await _productionStepCollectionService.GetProductionStepCollection(productionStepCollectionId);
        }

        [HttpPost]
        [Route("search")]
        public async Task<PageData<ProductionStepCollectionSearch>> SearchProductionStepCollection([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size = 0)
        {
            return await _productionStepCollectionService.SearchProductionStepCollection(keyword, page, size);
        }
    }
}
