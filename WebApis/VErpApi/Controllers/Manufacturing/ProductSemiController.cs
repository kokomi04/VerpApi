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
        private readonly IProductSemiConversionService _productSemiConversionService;

        public ProductSemiController(IProductSemiService productSemiService, IProductSemiConversionService productSemiConversionService)
        {
            _productSemiService = productSemiService;
            _productSemiConversionService = productSemiConversionService;
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
        [Route("searchByListProductSemiId")]
        public async Task<IList<ProductSemiModel>> GetListProductSemiListProductSemiId([FromBody] List<long> lsProductSemiId)
        {
            return await _productSemiService.GetListProductSemiListProductSemiId(lsProductSemiId);
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
        [Route("searchByListContainerId")]
        public async Task<IList<ProductSemiModel>> GetListProductSemiByListContainerId(IList<long> lstContainerId)
        {
            return await _productSemiService.GetListProductSemiByListContainerId(lstContainerId);
        }

        [HttpPost]
        [Route("{productSemiId}/conversions")]
        public async Task<long> AddProductSemiConversion([FromRoute] long productSemiId, [FromBody]ProductSemiConversionModel model)
        {
            model.ProductSemiId = productSemiId;
            return await _productSemiConversionService.AddProductSemiConversion(model);
        }

        [HttpPut]
        [Route("{productSemiId}/conversions/{conversionId}")]
        public async Task<bool> UpdateProductSemiConversion([FromRoute] long productSemiId, [FromRoute] long conversionId, [FromBody] ProductSemiConversionModel model)
        {
            model.ProductSemiId = productSemiId;
            return await _productSemiConversionService.UpdateProductSemiConversion(conversionId, model);
        }

        [HttpDelete]
        [Route("{productSemiId}/conversions/{conversionId}")]
        public async Task<bool> DeleteProductSemiConversion([FromRoute] long productSemiId, [FromRoute] long conversionId)
        {
            return await _productSemiConversionService.DeleteProductSemiConversion(conversionId);
        }

        [HttpGet]
        [Route("{productSemiId}/conversions")]
        public async Task<ICollection<ProductSemiConversionModel>> GetAllProductSemiConversionsByProductSemi([FromRoute] long productSemiId)
        {
            return await _productSemiConversionService.GetAllProductSemiConversionsByProductSemi(productSemiId);
        }

        [HttpPost]
        [Route("more")]
        public async Task<long[]> CreateListProductSemi([FromBody] IList<ProductSemiModel> models)
        {
            if(models.Count > 0)
                return await _productSemiService.CreateListProductSemi(models);
            return new long[] { };
        }

        [HttpPut]
        [Route("more")]
        public async Task<bool> UpdateListProductSemi([FromBody] IList<ProductSemiModel> models)
        {
            if (models.Count > 0)
                return await _productSemiService.UpdateListProductSemi(models);
            return true;
        }
    }
}
