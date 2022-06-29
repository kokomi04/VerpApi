using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductBomController : CrossServiceBaseController
    {
        private readonly IProductBomService _productBomService;
        public InternalProductBomController(IProductBomService productBomService)
        {
            _productBomService = productBomService;
        }


        [HttpPost]
        [Route("products")]
        public async Task<IList<ProductElementModel>> GetElements([FromBody] int[] productIds)
        {
            return await _productBomService.GetProductElements(productIds);
        }

        [HttpGet]
        [Route("{productId}")]
        [GlobalApi]
        public async Task<IList<ProductBomOutput>> GetBOM([FromRoute] int productId)
        {
            return await _productBomService.GetBom(productId);
        }

        [HttpPost]
        [Route("ByProductIds")]
        public async Task<IDictionary<int, IList<ProductBomOutput>>> ByProductIds([FromBody] IList<int> productIds)
        {
            return await _productBomService.GetBoms(productIds);
        }
    }
}