using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Products;

namespace VErp.WebApis.VErpApi.Controllers.Stock.Products
{
    public class ProductUnitConversionController : VErpBaseController
    {
        private readonly IProductUnitConversionService _productUnitConversionService;
        public ProductUnitConversionController(IProductUnitConversionService productUnitConversionService)
        {
            _productUnitConversionService = productUnitConversionService;
        }

        /// <summary>
        /// Thêm đơn vị quản lý mới cho sản phẩm
        /// </summary>
        /// <param name="productUnitConversion"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse> AddProductUnitConversion([FromBody] ProductUnitConversionModel productUnitConversion)
        {
            return await _productUnitConversionService.AddProductUnitConversion(productUnitConversion);
        }

        /// <summary>
        /// Lấy danh sach đơn vị theo sản phẩm
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{productId}")]
        public async Task<ApiResponse<List<ProductUnitConversionOutput>>> GetProductUnitConversionList([FromRoute] int productId)
        {
            return await _productUnitConversionService.ProductUnitConversionList(productId);
        }

        /// <summary>
        /// Cập nhật sản phẩm cho đơn vị tương ứng
        /// </summary>
        /// <param name="productUnitConversion"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<ApiResponse> UpdateProductUnitConversion([FromBody] ProductUnitConversionModel productUnitConversion)
        {
            return await _productUnitConversionService.UpdateProductUnitConversion(productUnitConversion);
        }

        /// <summary>
        /// Xóa sản phẩm theo từng đơn vị
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="secondaryUnitId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{productId}/{secondaryUnitId}")]
        public async Task<ApiResponse> Delete([FromRoute] int productId, [FromRoute]int secondaryUnitId)
        {
            return await _productUnitConversionService.DeleteProductUnitConversion(productId, secondaryUnitId);
        }
    }
}
