using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductSemi;
using VErp.Services.Manafacturing.Service.ProductSemi;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/ProductSemi")]
    [ApiController]
    public class ProductSemiController: ControllerBase
    {
        private readonly IProductSemiService _productSemiService;

        public ProductSemiController(IProductSemiService productSemiService)
        {
            _productSemiService = productSemiService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ProductSemiModel>> GetListProductSemi([FromQuery]long containerId, [FromQuery] int containerTypeId)
        {
            return await _productSemiService.GetListProductSemi(containerId, containerTypeId);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateProductSemi([FromBody] ProductSemiModel model)
        {
            return await _productSemiService.CreateProductSemi(model);
        }

        [HttpPut]
        [Route("{productSemiId}")]
        public async Task<bool> UpdateProductSemi([FromRoute] long productSemiId, [FromBody] ProductSemiModel model)
        {
            return await _productSemiService.UpdateProductSemi(productSemiId, model);
        }

        [HttpDelete]
        [Route("")]
        public async Task<bool> DeleteProductSemi([FromQuery] long productSemiId)
        {
            return await _productSemiService.DeleteProductSemi(productSemiId);
        }
    }
}
