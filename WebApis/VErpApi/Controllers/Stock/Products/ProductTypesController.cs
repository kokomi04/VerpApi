using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Service.Dictionary;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/productTypes")]

    public class ProductTypesController : VErpBaseController
    {
        private readonly IProductTypeService _productTypeService;
        public ProductTypesController(IProductTypeService productTypeService
            )
        {
            _productTypeService = productTypeService;
        }

        /// <summary>
        /// Lấy danh sách loại sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<ProductTypeOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productTypeService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Thêm mới loại sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddProductType([FromBody] ProductTypeInput type)
        {
            return await _productTypeService.AddProductType(type);
        }

        /// <summary>
        /// Lấy thông tin loại sản phẩm
        /// </summary>
        /// <param name="productTypeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{productTypeId}")]
        public async Task<ApiResponse<ProductTypeOutput>> GetProductTypeInfo([FromRoute] int productTypeId)
        {
            return await _productTypeService.GetInfoProductType(productTypeId);
        }

       /// <summary>
       /// Cập nhật thông tin loại sản phẩm
       /// </summary>
       /// <param name="productTypeId"></param>
       /// <param name="type"></param>
       /// <returns></returns>
        [HttpPut]
        [Route("{productTypeId}")]
        public async Task<ApiResponse> UpdateProductType([FromRoute] int productTypeId, [FromBody] ProductTypeInput type)
        {
            return await _productTypeService.UpdateProductType(productTypeId, type);
        }

        /// <summary>
        /// Xóa danh loại sản phẩm
        /// </summary>
        /// <param name="productTypeId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{productTypeId}")]
        public async Task<ApiResponse> DeleteProductType([FromRoute] int productTypeId)
        {
            return await _productTypeService.DeleteProductType(productTypeId);
        }       
    }
}