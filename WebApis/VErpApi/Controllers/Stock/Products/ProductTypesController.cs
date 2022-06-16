﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
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
        public async Task<PageData<ProductTypeOutput>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productTypeService.GetList(keyword, page, size).ConfigureAwait(true);
        }

        /// <summary>
        /// Thêm mới loại sản phẩm
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<int> AddProductType([FromBody] ProductTypeInput type)
        {
            return await _productTypeService.AddProductType(type).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin loại sản phẩm
        /// </summary>
        /// <param name="productTypeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{productTypeId}")]
        public async Task<ProductTypeOutput> GetProductTypeInfo([FromRoute] int productTypeId)
        {
            return await _productTypeService.GetInfoProductType(productTypeId).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật thông tin loại sản phẩm
        /// </summary>
        /// <param name="productTypeId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{productTypeId}")]
        public async Task<bool> UpdateProductType([FromRoute] int productTypeId, [FromBody] ProductTypeInput type)
        {
            return await _productTypeService.UpdateProductType(productTypeId, type).ConfigureAwait(true);
        }

        /// <summary>
        /// Xóa danh loại sản phẩm
        /// </summary>
        /// <param name="productTypeId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{productTypeId}")]
        public async Task<bool> DeleteProductType([FromRoute] int productTypeId)
        {
            return await _productTypeService.DeleteProductType(productTypeId).ConfigureAwait(true);
        }
    }
}