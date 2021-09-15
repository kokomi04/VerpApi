using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Model.ProductPrice;
using VErp.Services.PurchaseOrder.Service.ProductPrice;
using VErp.Services.PurchaseOrder.Service.ProductPrice.Implement;

namespace VErpApi.Controllers.PurchaseOrder.ProductPrice
{
    [Route("api/ProductPrice/ItemPricing")]
    public class ProductPriceConfigItemPricingController : VErpBaseController
    {
        private readonly IProductPriceItemPricingService _productPriceItemPricingService;
        public ProductPriceConfigItemPricingController(IProductPriceItemPricingService productPriceItemPricingService)
        {
            _productPriceItemPricingService = productPriceItemPricingService;
        }

        [HttpGet("{productPriceConfigId}")]
        public async Task<ProductPriceConfigItemPricingUpdate> GetConfigItemPricing([FromRoute] int productPriceConfigId)
        {
            return await _productPriceItemPricingService.GetConfigItemPricing(productPriceConfigId);
        }

        [HttpPut("{productPriceConfigId}")]
        public async Task<bool> UpdateConfigItemPricing([FromRoute] int productPriceConfigId, ProductPriceConfigItemPricingUpdate model)
        {
            return await _productPriceItemPricingService.UpdateConfigItemPricing(productPriceConfigId, model);
        }

    }
}