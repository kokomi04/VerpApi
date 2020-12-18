using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
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
using VErp.Commons.GlobalObject;
using Newtonsoft.Json;
using VErp.Commons.Library.Model;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/products")]

    public class ProductsController : VErpBaseController
    {
        private readonly IProductService _productService;
        private readonly IFileService _fileService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IProductTypeService _productTypeService;
        private readonly IGenCodeConfigService _genCodeConfigService;

        public ProductsController(
            IProductService productService
            , IFileService fileService
            , IObjectGenCodeService objectGenCodeService
            , IProductTypeService productTypeService
            , IGenCodeConfigService genCodeConfigService
            )
        {
            _productService = productService;
            _fileService = fileService;
            _objectGenCodeService = objectGenCodeService;
            _productTypeService = productTypeService;
            _genCodeConfigService = genCodeConfigService;
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
        public async Task<PageData<ProductListOutput>> Search([FromQuery] string keyword, [FromQuery] string productName, [FromQuery] int page, [FromQuery] int size, [FromQuery] int[] productTypeIds = null, [FromQuery] int[] productCateIds = null)
        {
            return await _productService.GetList(keyword, productName, productTypeIds, productCateIds, page, size);
        }


        [HttpGet]
        [Route("fields")]
        public List<EntityField> GetFields()
        {
            return _productService.GetFields(typeof(ProductImportModel));
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<int> ImportFromMapping([FromForm] string mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _productService.ImportProductFromMapping(JsonConvert.DeserializeObject<ImportExcelMapping>(mapping), file.OpenReadStream()).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm theo ids
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetByIds")]
        [VErpAction(EnumAction.View)]
        public async Task<IList<ProductListOutput>> GetByIds([FromBody] IList<int> productIds)
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
        public async Task<int> AddProduct([FromBody] ProductModel product)
        {
            return await _productService.AddProduct(product);
        }

        /// <summary>
        /// Thêm mới sản phẩm vào danh mục mặc định
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("default")]
        public async Task<ProductDefaultModel> AddProductDefault([FromBody] ProductDefaultModel product)
        {
            return await _productService.AddProductDefault(product);
        }

        /// <summary>
        /// Lấy thông tin sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{productId}")]
        public async Task<ProductModel> GetProduct([FromRoute] int productId)
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
        public async Task<bool> UpdateProduct([FromRoute] int productId, [FromBody] ProductModel product)
        {
            return await _productService.UpdateProduct(productId, product);
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{productId}")]
        public async Task<bool> Delete([FromRoute] int productId)
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
        public async Task<long> UploadImage([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.Product, fileTypeId, string.Empty, file);
        }
    }
}