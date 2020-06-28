using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/products")]

    public class ProductsController : VErpBaseController
    {
        private readonly IProductService _productService;
        private readonly IFileService _fileService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IProductTypeService _productTypeService;
        private readonly ICustomGenCodeService _customGenCodeService;

        public ProductsController(
            IProductService productService
            , IFileService fileService
            , IObjectGenCodeService objectGenCodeService
            , IProductTypeService productTypeService
            , ICustomGenCodeService customGenCodeService
            )
        {
            _productService = productService;
            _fileService = fileService;
            _objectGenCodeService = objectGenCodeService;
            _productTypeService = productTypeService;
            _customGenCodeService = customGenCodeService;
        }

        /// <summary>
        /// Tìm kiếm sản phẩm
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="productTypeIds"></param>
        /// <param name="productCateIds"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<ProductListOutput>>> Search([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int[] productTypeIds = null, [FromQuery] int[] productCateIds = null)
        {
            return await _productService.GetList(keyword, productTypeIds, productCateIds, page, size);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm theo ids
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetByIds")]
        [VErpAction(EnumAction.View)]
        public async Task<ServiceResult<IList<ProductListOutput>>> GetByIds([FromBody] IList<int> productIds)
        {
            return (await _productService.GetListByIds(productIds)).ToList();
        }

        /// <summary>
        /// Thêm mới sản phẩm
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> AddProduct([FromBody] ProductModel product)
        {
            return await UpdateOrAddProduct(null, product);
        }

        /// <summary>
        /// Lấy thông tin sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{productId}")]
        public async Task<ServiceResult<ProductModel>> GetProduct([FromRoute] int productId)
        {
            return await _productService.ProductInfo(productId);
        }

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{productId}")]
        public async Task<ServiceResult> UpdateProduct([FromRoute] int productId, [FromBody] ProductModel product)
        {
            return await UpdateOrAddProduct(productId, product);
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{productId}")]
        public async Task<ServiceResult> Delete([FromRoute] int productId)
        {
            return await _productService.DeleteProduct(productId);
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="fileTypeId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("File/{fileTypeId}")]
        public async Task<ServiceResult<long>> UploadImage([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.Product, fileTypeId, string.Empty, file);
        }

        /// <summary>
        /// Sinh mã sản phẩm
        /// </summary>
        /// <param name="productTypeId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateProductCode")]
        public async Task<ServiceResult<string>> GenerateProductCode([FromQuery] int? productTypeId)
        {
            var typeCode = "";
            if (productTypeId.HasValue)
            {
                var typeInfo = await _productTypeService.GetInfoProductType(productTypeId.Value);
                if (!typeInfo.Code.IsSuccess())
                {
                    return typeInfo.Code;
                }
                typeCode = typeInfo.Data.IdentityCode;
            }
            var objectCode = await _objectGenCodeService.GenerateCode(EnumObjectType.Product);
            if (!objectCode.Code.IsSuccess())
            {
                return objectCode;
            }

            objectCode.Data = string.IsNullOrWhiteSpace(typeCode) ? objectCode.Data : typeCode + objectCode.Data;

            return objectCode;
        }


        private async Task<ServiceResult<int>> UpdateOrAddProduct(int? productId, ProductModel product)
        {
            // var lastValue = 0;
            var isGenCode = false;
            if (string.IsNullOrWhiteSpace(product?.ProductCode) && product.ProductTypeId.HasValue)
            {
                var productTypeInfo = await _productTypeService.GetInfoProductType(product.ProductTypeId.Value);
                if (!productTypeInfo.Code.IsSuccess())
                {
                    return productTypeInfo.Code;
                }

                var productTypeConfig = await _customGenCodeService.GetCurrentConfig((int)EnumObjectType.ProductType, product.ProductTypeId.Value).ConfigureAwait(true);
                if (productTypeConfig.Code.IsSuccess())
                {
                    var code = await _customGenCodeService.GenerateCode(productTypeConfig.Data.CustomGenCodeId, 0, productTypeInfo.Data.IdentityCode).ConfigureAwait(true);
                    if (!code.Code.IsSuccess())
                    {
                        return code.Code;
                    }

                    product.ProductCode = code.Data.CustomCode;
                    // lastValue = code.Data.LastValue;
                    isGenCode = true;
                }
            }

            ServiceResult<int> r;
            if (!productId.HasValue)
            {
                r = await _productService.AddProduct(product).ConfigureAwait(true);
            }
            else
            {
                r = await _productService.UpdateProduct(productId.Value, product).ConfigureAwait(true);
            }

            if (isGenCode && r.Code.IsSuccess())
            {
                await _customGenCodeService.ConfirmCode((int)EnumObjectType.ProductType, product.ProductTypeId.Value).ConfigureAwait(true);
            }
            return r;
        }
    }
}