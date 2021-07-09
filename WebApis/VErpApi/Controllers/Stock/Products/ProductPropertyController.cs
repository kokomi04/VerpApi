using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/productProperty")]
    public class ProductPropertyController : VErpBaseController
    {
        private readonly IProductPropertyService _productPropertyService;
        public ProductPropertyController(IProductPropertyService productPropertyService)
        {
            _productPropertyService = productPropertyService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ProductPropertyModel>> GetProductProperties()
        {
            return await _productPropertyService.GetProductProperties();
        }

        [HttpGet]
        [Route("{productPropertyId}")]
        public async Task<ProductPropertyModel> GetProductProperty([FromRoute] int productPropertyId)
        {
            return await _productPropertyService.GetProductProperty(productPropertyId);
        }


        [HttpPost]
        [Route("")]
        public async Task<int> CreateProductProperty([FromBody] ProductPropertyModel model)
        {
            return await _productPropertyService.CreateProductProperty(model);
        }

        [HttpPut]
        [Route("{productPropertyId}")]
        public async Task<int> UpdateProductProperty([FromRoute] int productPropertyId, [FromBody] ProductPropertyModel model)
        {
            return await _productPropertyService.UpdateProductProperty(productPropertyId, model);
        }

        [HttpDelete]
        [Route("{productPropertyId}")]
        public async Task<bool> DeleteProductProperty([FromRoute] int productPropertyId)
        {
            return await _productPropertyService.DeleteProductProperty(productPropertyId);
        }
    }
}
