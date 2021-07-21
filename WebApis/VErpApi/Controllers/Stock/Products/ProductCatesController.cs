using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Service.Dictionary;

namespace VErpApi.Controllers.Stock.Products
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
        [GlobalApi]
        public async Task<PageData<ProductCateOutput>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderBy, [FromQuery] bool asc)
        {
            return await _productCateService.GetList(keyword, page, size, orderBy, asc).ConfigureAwait(true);
        }

        /// <summary>
        /// Thêm mới danh mục sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<int> AddProductCate([FromBody] ProductCateInput productCate)
        {
            return await _productCateService.AddProductCate(productCate).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin danh mục sản phẩm
        /// </summary>
        /// <param name="productCateId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{productCateId}")]
        public async Task<ProductCateOutput> GetProductCateInfo([FromRoute] int productCateId)
        {
            return await _productCateService.GetInfoProductCate(productCateId).ConfigureAwait(true);
        }

       /// <summary>
       /// Cập nhật thông tin danh mục sản phẩm
       /// </summary>
       /// <param name="productCateId"></param>
       /// <param name="productCate"></param>
       /// <returns></returns>
        [HttpPut]
        [Route("{productCateId}")]
        public async Task<bool> UpdateProductCate([FromRoute] int productCateId, [FromBody] ProductCateInput productCate)
        {
            return await _productCateService.UpdateProductCate(productCateId, productCate).ConfigureAwait(true);
        }

        /// <summary>
        /// Xóa danh mục sản phẩm
        /// </summary>
        /// <param name="productCateId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{productCateId}")]
        public async Task<bool> DeleteProductCate([FromRoute] int productCateId)
        {
            return await _productCateService.DeleteProductCate(productCateId).ConfigureAwait(true);
        }
    }
}