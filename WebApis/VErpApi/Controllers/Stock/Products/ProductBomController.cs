using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/productBom")]
    public class ProductBomController: VErpBaseController
    {
        private readonly IProductBomService _productBomService;
        public ProductBomController(IProductBomService productBomService
            )
        {
            _productBomService = productBomService;
        }

        [HttpGet]
        [Route("{productId}")]
        public async Task<IList<ProductBomOutput>> GetBOM([FromRoute] int productId)
        {
            return await _productBomService.GetBOM(productId);
        }
        
        [HttpPut]
        [Route("{productId}")]
        public async Task<bool> Update([FromRoute] int productId ,[FromBody] IList<ProductBomInput> model)
        {
            return await _productBomService.Update(productId, model);
        }
    }
}
