using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/productBom")]
    public class ProductBomController: VErpBaseController
    {
        private readonly IProductBomService _productBomService;
        public ProductBomController(IProductBomService productBomService
            )
        {
            _productBomService = productBomService;
        }

        /// <summary>
        /// Lấy thông tin 1 bom theo mã
        /// </summary>
        /// <param name="productBomId">Id của 1 bom</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ProductBomOutput> Get([FromQuery] int productBomId)
        {
            return await _productBomService.Get(productBomId);
        }

        /// <summary>
        /// Lấy danh sách (toàn bộ) vật tư và chi tiết cấu thành (Bom) của sản phẩm theo ProductId
        /// </summary>
        /// <param name="productId">Mã Id SPVTHH</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAll")]
        public async Task<PageData<ProductBomOutput>> GetAll([FromQuery] int productId)
        {
            return await _productBomService.GetAll(productId);
        }

        /// <summary>
        /// Thêm mới thông tin vào ProductBom
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<long> Add([FromBody] ProductBomInput model)
        {
            return await _productBomService.Add(model);
        }

        /// <summary>
        /// Cập nhật thông tin 
        /// </summary>
        /// <param name="productBomId">Id của productBom</param>
        /// <param name="model">input productBom model </param>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public async Task<bool> Update([FromQuery] long productBomId ,[FromBody] ProductBomInput model)
        {
            return await _productBomService.Update(productBomId, model);
        }

        /// <summary>
        /// Xoá thông tin productBom của sản phẩm
        /// </summary>
        /// <param name="productBomId">Id của bom</param>
        /// <param name="rootProductId">Id của sản phẩm có chứa bom đó</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public async Task<bool> Delete([FromQuery] long productBomId,[FromQuery] int rootProductId)
        {
            return await _productBomService.Delete(productBomId, rootProductId);
        }

    }
}
