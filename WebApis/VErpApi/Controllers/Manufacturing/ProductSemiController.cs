using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductSemi;
using VErp.Services.Manafacturing.Service.ProductSemi;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/ProductSemi")]
    [ApiController]
    public class ProductSemiController: VErpBaseController
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

        [HttpGet]
        [Route("{productSemiId}")]
        public async Task<ProductSemiModel> GetListProductSemiById([FromRoute] long productSemiId)
        {
            return await _productSemiService.GetListProductSemiById(productSemiId);
        }

        [HttpPost]
        [Route("getByContainerId")]
        public async Task<IList<ProductSemiModel>> GetListProductSemiByListId([FromBody] List<long> containerId)
        {
            return await _productSemiService.GetListProductSemiByListId(containerId);
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
        [Route("{productSemiId}")]
        public async Task<bool> DeleteProductSemi([FromRoute] long productSemiId)
        {
            return await _productSemiService.DeleteProductSemi(productSemiId);
        }

        [HttpPost]
        [Route("/searchByListContainerId")]
        public async Task<IList<ProductSemiModel>> GetListProductSemiByListContainerId(IList<long> lstContainerId)
        {
            return await _productSemiService.GetListProductSemi(lstContainerId);
        }
    }
}
