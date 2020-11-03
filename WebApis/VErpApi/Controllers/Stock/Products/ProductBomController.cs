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
        [Route("")]
        public async Task<IList<ProductBomOutput>> GetBOM([FromQuery] int productBomId)
        {
            return await _productBomService.GetBOM(productBomId);
        }
        
        [HttpPost]
        [Route("")]
        public async Task<long> Add([FromBody] ProductBomInput model)
        {
            return await _productBomService.Add(model);
        }

        [HttpPut]
        [Route("")]
        public async Task<bool> Update([FromQuery] long productBomId ,[FromBody] ProductBomInput model)
        {
            return await _productBomService.Update(productBomId, model);
        }

        [HttpDelete]
        [Route("")]
        public async Task<bool> Delete([FromQuery] long productBomId,[FromQuery] int rootProductId)
        {
            return await _productBomService.Delete(productBomId, rootProductId);
        }

    }
}
