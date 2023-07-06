using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductController : CrossServiceBaseController
    {
        private readonly IProductService _productService;
        public InternalProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<ProductListOutput>> Search([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] string productName, [FromQuery] int page, [FromQuery] int size, [FromQuery] int[] productTypeIds = null, [FromQuery] int[] productCateIds = null,
            [FromQuery] bool? isProductSemi = null, [FromQuery] bool? isProduct = null, [FromQuery] bool? isMaterials = null,
            [FromQuery] EnumProductionProcessStatus? productionProcessStatusId = null
            )
        {
            var req = new ProductFilterRequestModel(keyword, productIds, productName, productTypeIds, productCateIds, isProductSemi, isProduct, isMaterials, productionProcessStatusId, filters);
            //return await _productService.GetList(keyword, productIds, productName, productTypeIds, productCateIds, page, size, isProductSemi, isProduct, isMaterials, filters);
            return await _productService.GetList(req, page, size);
        }


        [HttpGet]
        [Route("{productId}")]
        public async Task<ProductModel> GetProduct([FromRoute] int productId)
        {
            return await _productService.ProductInfo(productId);
        }

        [HttpPost]
        [Route("GetByIds")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<ProductListOutput>> GetByIds([FromBody] IList<int> productIds)
        {
            return (await _productService.GetListByIds(productIds)).ToList();
        }

        [HttpPost]
        [Route("validateProductUnitConversion")]
        public async Task<bool> ValidateProductUnitConversion([FromBody] Dictionary<int, int> productUnitConvertsionProduct)
        {
            return await _productService.ValidateProductUnitConversions(productUnitConvertsionProduct).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("GetListByCodeAndInternalNames")]
        public async Task<IList<ProductModel>> GetListByCodeAndInternalNames([FromBody] ProductQueryByProductCodeOrInternalNameRequest req)
        {
            return await _productService.GetListByCodeAndInternalNames(req);
        }

        [HttpPost]
        [Route("GetListProductsByIds")]
        public async Task<IList<ProductModel>> GetListProducts([FromBody] IList<int> productIds)
        {
            return await _productService.GetListProductsByIds(productIds);
        }

        [HttpPut]
        [Route("{productId}/coefficient")]
        public async Task<bool> UpdateProductCoefficientManual([FromRoute] int productId, [FromQuery] decimal coefficient)
        {
            return await _productService.UpdateProductCoefficientManual(productId, coefficient);
        }

        [HttpPut]
        [Route("{productId}/productionProcessVersion")]
        public async Task<bool> UpdateProductionProcessVersion([FromRoute] int productId)
        {
            return await _productService.UpdateProductionProcessVersion(productId);
        }

        [HttpGet]
        [Route("{productId}/productionProcessVersion")]
        public async Task<long> GetProductionProcessVersion([FromRoute] int productId)
        {
            return await _productService.GetProductionProcessVersion(productId);
        }

        [HttpPut]
        [Route("productProcessStatus")]
        public async Task<bool> UpdateProductProcessStatus([FromBody] InternalProductProcessStatus productProcessStatus, [FromQuery] bool isSaveLog)
        {
            return await _productService.UpdateProductProcessStatus(productProcessStatus, isSaveLog);
        }
    }
}