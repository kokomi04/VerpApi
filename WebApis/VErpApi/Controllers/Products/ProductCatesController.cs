using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Service.Dictionary;

namespace VErpApi.Controllers.System
{
    [Route("api/productCates")]

    public class ProductCatesController : VErpBaseController
    {
        private readonly IProductCateService _productCateService;
        public ProductCatesController(IProductCateService productCateService
            )
        {
            _productCateService = productCateService;
        }

        /// <summary>
        /// Lấy danh sách danh mục sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<ProductCateOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productCateService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Thêm mới danh mục sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddProductCate([FromBody] ProductCateInput productCate)
        {
            return await _productCateService.AddProductCate(productCate);
        }

        /// <summary>
        /// Lấy thông tin danh mục sản phẩm
        /// </summary>
        /// <param name="productCateId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{productCateId}")]
        public async Task<ApiResponse<ProductCateOutput>> GetProductCateInfo([FromRoute] int productCateId)
        {
            return await _productCateService.GetInfoProductCate(productCateId);
        }

       /// <summary>
       /// Cập nhật thông tin danh mục sản phẩm
       /// </summary>
       /// <param name="productCateId"></param>
       /// <param name="productCate"></param>
       /// <returns></returns>
        [HttpPut]
        [Route("{productCateId}")]
        public async Task<ApiResponse> UpdateProductCate([FromRoute] int productCateId, [FromBody] ProductCateInput productCate)
        {
            return await _productCateService.UpdateProductCate(productCateId, productCate);
        }

        /// <summary>
        /// Xóa danh mục sản phẩm
        /// </summary>
        /// <param name="productCateId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{productCateId}")]
        public async Task<ApiResponse> DeleteProductCate([FromRoute] int productCateId)
        {
            return await _productCateService.DeleteProductCate(productCateId);
        }       
    }
}