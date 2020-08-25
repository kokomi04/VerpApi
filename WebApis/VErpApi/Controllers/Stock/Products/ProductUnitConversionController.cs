using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/productunitconversion")]
    public class ProductUnitConversionController : VErpBaseController
    {
        private readonly IProductUnitConversionService _productUnitConversionService;

        public ProductUnitConversionController(IProductUnitConversionService productUnitConversionService)
        {
            _productUnitConversionService = productUnitConversionService;

        }

        /// <summary>
        /// Lấy danh sách tỉ lệ chuyển đổi của các đơn vị thay thế
        /// </summary>
        /// <param name="productId">Id của sản phẩm</param>        
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PageData<ProductUnitConversionOutput>> Get([FromQuery] int productId, [FromQuery] int page = 0, [FromQuery] int size = 0)
        {
            return await _productUnitConversionService.GetList(productId: productId, page: page, size: size);
        }

        /// <summary>
        /// Lấy danh sách đơn vị chuyển đổi của danh sách mặt hàng
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ByProducts")]
        public async Task<PageData<ProductUnitConversionByProductOutput>> ByProducts([FromQuery] IList<int> productIds, [FromQuery] int page = 0, [FromQuery] int size = 0)
        {
            return await _productUnitConversionService.GetListByProducts(productIds, page, size);
        }


        /// <summary>
        /// Lấy danh sách đơn vị chuyển đổi của danh sách mặt hàng
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetByProducts")]
        [VErpAction(EnumAction.View)]
        public async Task<PageData<ProductUnitConversionByProductOutput>> GetByProducts([FromBody] IList<int> productIds, [FromQuery] int page = 0, [FromQuery] int size = 0)
        {
            return await _productUnitConversionService.GetListByProducts(productIds, page, size);
        }
    }
}
