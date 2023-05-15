using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Product.Partial;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;

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


        //[HttpPost]
        //[Route("Search")]
        //[GlobalApi]
        //[VErpAction(EnumActionType.View)]
        //public async Task<PageData<ProductListOutput>> Search([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] string productName, [FromQuery] int page, [FromQuery] int size, 
        //    [FromQuery] int[] productTypeIds = null, [FromQuery] int[] productCateIds = null, 
        //    [FromQuery] bool? isProductSemi = null, [FromQuery] bool? isProduct = null, [FromQuery] bool? isMaterials = null,
        //    [FromQuery] EnumProductionProcessStatus? productionProcessStatusId = null,
        //    [FromBody] Clause filters = null)
        //{
        //    var req = new ProductFilterRequestModel(keyword, productIds, productName, productTypeIds, productCateIds, isProductSemi: isProductSemi, isProduct: isProduct, isMaterials: isMaterials, productionProcessStatusId: productionProcessStatusId, filters);
        //    //return await _productService.GetList(keyword, productIds, productName, productTypeIds, productCateIds, page, size, isProductSemi: isProductSemi, isProduct: isProduct, isMaterials: isMaterials, filters);
        //    return await _productService.GetList(req, page, size);
        //}


        [HttpPost]
        [Route("SearchV2")]
        [GlobalApi]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<ProductListOutput>> Search([FromBody] ProductSearchRequestModel req)
        {
            return await _productService.GetList(req, req.Page, req.Size);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("ExportList")]
        public async Task<IActionResult> ExportList([FromBody] ProductExportRequestModel req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            var (stream, fileName, contentType) = await _productService.ExportList(req);

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
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
         
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
        [GlobalApi]
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
        public async Task<bool> UpdateGeneralInfo([FromRoute] int productId, [FromBody] ProductPartialGeneralUpdateWithExtraModel model)
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


        [HttpGet]
        [Route("{productId}/ProcessInfo")]
        public async Task<ProductProcessModel> ProcessInfo([FromRoute] int productId)
        {
            return await _productPartialService.ProcessInfo(productId);
        }


        [HttpPut]
        [Route("{productId}/ProcessInfo")]
        public async Task<bool> ProcessInfo([FromRoute] int productId, [FromBody] ProductProcessModel model)
        {
            return await _productPartialService.UpdateProcessInfo(productId, model);
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


        [HttpPost]
        [Route("File/{fileTypeId}")]
        public async Task<long> UploadImage([FromRoute] EnumFileType? fileTypeId, [FromForm] IFormFile file)
        {
            if (fileTypeId == null) throw new BadRequestException(GeneralCode.InvalidParams);
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
        public async Task<long> CopyProductBom([FromQuery] int sourceProductId, [FromQuery] int destProductId)
        {
            return await _productService.CopyProductBom(sourceProductId, destProductId);
        }

        [HttpPost]
        [Route("Copy/MaterialsConsumption")]
        public async Task<long> CopyProductMaterialConsumption([FromQuery] int sourceProductId, [FromQuery] int destProductId)
        {
            return await _productService.CopyProductMaterialConsumption(sourceProductId, destProductId);
        }

        [HttpGet]
        [Route("{productId}/productionProcessVersion")]
        public async Task<long> GetProductionProcessVersion([FromRoute] int productId)
        {
            return await _productService.GetProductionProcessVersion(productId);
        }


        [HttpPost]
        [Route("GetProductTopInUsed")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<ProductInUsedInfo>> GetProductTopInUsed([FromBody] IList<int> productIds)
        {
            return (await _productService.GetProductTopInUsed(productIds, false)).ToList();
        }
    }
}