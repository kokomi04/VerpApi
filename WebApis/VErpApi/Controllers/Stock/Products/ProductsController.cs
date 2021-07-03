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
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Services.Stock.Model.Product.Partial;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/products")]

    public class ProductsController : VErpBaseController
    {
        private readonly IProductService _productService;
        private readonly IFileService _fileService;
        private readonly IProductPartialService _productPartialService;
        public ProductsController(
            IProductService productService
            , IFileService fileService
            , IProductPartialService productPartialService
            )
        {
            _productService = productService;
            _fileService = fileService;
            _productPartialService = productPartialService;
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
        public async Task<PageData<ProductListOutput>> Search([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] string productName, [FromQuery] int page, [FromQuery] int size, [FromQuery] int[] productTypeIds = null, [FromQuery] int[] productCateIds = null, [FromQuery] bool? isProductSemi = null, [FromQuery] bool? isProduct = null, [FromQuery] bool? isMaterials = null)
        {
            return await _productService.GetList(keyword, productIds, productName, productTypeIds, productCateIds, page, size, isProductSemi: isProductSemi, isProduct: isProduct, isMaterials: isMaterials);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("ExportList")]
        public async Task<IActionResult> ExportList([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] string productName, [FromQuery] int page, [FromQuery] int size, [FromQuery] int[] productTypeIds = null, [FromQuery] int[] productCateIds = null, [FromQuery] bool? isProductSemi = null, [FromQuery] bool? isProduct = null, [FromQuery] bool? isMaterials = null)
        {
            var (stream, fileName, contentType) =  await _productService.ExportList(keyword, productIds, productName, productTypeIds, productCateIds, page, size, isProductSemi: isProductSemi, isProduct: isProduct, isMaterials: isMaterials);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }


        [HttpGet]
        [Route("fields")]
        public CategoryNameModel GetFieldMappings()
        {
            return _productService.GetFieldMappings();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _productService.ImportProductFromMapping(mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm theo ids
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetByIds")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
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


        [HttpPost]
        [Route("{parentProductId}/SemiProduct")]
        public async Task<ProductDefaultModel> AddProductSemiProduct([FromRoute] int parentProductId, [FromBody] ProductDefaultModel product)
        {
            return await _productService.ProductAddProductSemi(parentProductId, product);
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

        [HttpGet]
        [Route("{productId}/GeneralInfo")]
        public async Task<ProductPartialGeneralModel> GeneralInfo([FromRoute] int productId)
        {
            return await _productPartialService.GeneralInfo(productId);
        }

        [HttpPut]
        [Route("{productId}/GeneralInfo")]
        public async Task<bool> UpdateGeneralInfo([FromRoute] int productId, [FromBody] ProductPartialGeneralModel model)
        {
            return await _productPartialService.UpdateGeneralInfo(productId, model);
        }

        [HttpGet]
        [Route("{productId}/StockInfo")]
        public async Task<ProductPartialStockModel> StockInfo([FromRoute] int productId)
        {
            return await _productPartialService.StockInfo(productId);
        }

        [HttpPut]
        [Route("{productId}/StockInfo")]
        public async Task<bool> UpdateStockInfo([FromRoute] int productId, [FromBody] ProductPartialStockModel model)
        {
            return await _productPartialService.UpdateStockInfo(productId, model);
        }


        [HttpGet]
        [Route("{productId}/SellInfo")]
        public async Task<ProductPartialSellModel> SellInfo([FromRoute] int productId)
        {
            return await _productPartialService.SellInfo(productId);
        }

        [HttpPut]
        [Route("{productId}/SellInfo")]
        public async Task<bool> UpdateSellInfo([FromRoute] int productId, [FromBody] ProductPartialSellModel model)
        {
            return await _productPartialService.UpdateSellInfo(productId, model);
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
            return await _fileService.Upload(EnumObjectType.Product, string.Empty, file);
        }

        [HttpPost]
        [Route("Copy")]
        public async Task<long> CopyProduct([FromQuery] int sourceProductId, [FromBody] ProductModel product)
        {
            return await _productService.CopyProduct(product, sourceProductId);
        }

        [HttpPost]
        [Route("Copy/Bom")]
        public async Task<long> CopyProductBom([FromQuery] int sourceProductId, [FromQuery] int destProductId) {
            return await _productService.CopyProductBom(sourceProductId, destProductId);
        }

        [HttpPost]
        [Route("Copy/MaterialsConsumption")]
        public async Task<long> CopyProductMaterialConsumption([FromQuery] int sourceProductId, [FromQuery] int destProductId) {
            return await _productService.CopyProductMaterialConsumption(sourceProductId, destProductId);
        }
    }
}