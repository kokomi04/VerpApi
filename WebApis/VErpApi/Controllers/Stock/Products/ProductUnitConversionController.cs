﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product.Pu;
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
        [GlobalApi]
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
        [GlobalApi]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<ProductUnitConversionByProductOutput>> GetByProducts([FromBody] IList<int> productIds, [FromQuery] int page = 0, [FromQuery] int size = 0)
        {
            return await _productUnitConversionService.GetListByProducts(productIds, page, size);
        }


        [HttpPost]
        [Route("GetByInStockProducts")]
        [GlobalApi]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<ProductUnitConversionByProductOutput>> GetByInStockProducts([FromBody] IList<int> productIds, [FromQuery] int stockId, [FromQuery] long unixDate)
        {
            return await _productUnitConversionService.GetByInStockProducts(productIds, stockId, unixDate);
        }

        [HttpGet]
        [Route("fieldDataForImportMapping")]
        public CategoryNameModel GetFieldDataForImportMapping()
        {
            return _productUnitConversionService.GetFieldDataForImportMapping();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (mapping == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
            return await _productUnitConversionService.Import(mapping, file.OpenReadStream());
        }
    }
  
}
