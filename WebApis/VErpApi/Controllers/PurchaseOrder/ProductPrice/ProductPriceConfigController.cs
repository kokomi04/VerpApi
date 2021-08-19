using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Model.ProductPrice;
using VErp.Services.PurchaseOrder.Service.ProductPrice.Implement;

namespace VErpApi.Controllers.PurchaseOrder.ProductPrice
{
    [Route("api/ProductPrice/Config")]
    public class ProductPriceConfigController : VErpBaseController
    {
        private readonly IProductPriceConfigService _productPriceConfigService;
        public ProductPriceConfigController(IProductPriceConfigService productPriceConfigService)
        {
            _productPriceConfigService = productPriceConfigService;
        }

        [HttpGet]
        public async Task<IList<ProductPriceConfigVersionModel>> GetList([FromQuery] bool? isActived)
        {
            return await _productPriceConfigService.GetList(isActived);
        }


        [HttpPost]
        public async Task<int> Create([FromBody] ProductPriceConfigVersionModel model)
        {
            return await _productPriceConfigService.Create(model);
        }

        [HttpDelete("{productPriceConfigId}")]
        public async Task<bool> Delete([FromRoute] int productPriceConfigId)
        {
            return await _productPriceConfigService.Delete(productPriceConfigId);
        }

        [HttpPut("{productPriceConfigId}")]
        public async Task<int> Update([FromRoute] int productPriceConfigId, [FromBody] ProductPriceConfigVersionModel model)
        {
            return await _productPriceConfigService.Update(productPriceConfigId, model);
        }

        [HttpGet("{productPriceConfigId}")]
        public async Task<ProductPriceConfigVersionModel> LastestVersionInfo([FromRoute] int productPriceConfigId)
        {
            return await _productPriceConfigService.LastestVersionInfo(productPriceConfigId);
        }

        [HttpGet("VersionInfo/{productPriceConfigVersionId}")]
        public async Task<ProductPriceConfigVersionModel> VersionInfo([FromRoute] int productPriceConfigVersionId)
        {
            return await _productPriceConfigService.VersionInfo(productPriceConfigVersionId);
        }

    }
}