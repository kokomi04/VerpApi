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
        private readonly IBillOfMaterialService _billOfMaterialService;
        public ProductBomController(IBillOfMaterialService billOfMaterialService
            )
        {
            _billOfMaterialService = billOfMaterialService;
        }

        /// <summary>
        /// Lấy thông tin 1 bom theo mã
        /// </summary>
        /// <param name="billOfMaterialId">Id của 1 bom</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<ServiceResult<BillOfMaterialOutput>>> Get([FromQuery] int billOfMaterialId)
        {
            return await _billOfMaterialService.Get(billOfMaterialId);
        }

        /// <summary>
        /// Lấy danh sách (toàn bộ) vật tư và chi tiết cấu thành (Bom) của sản phẩm theo ProductId
        /// </summary>
        /// <param name="productId">Mã Id SPVTHH</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAll")]
        public async Task<ApiResponse<PageData<BillOfMaterialOutput>>> GetAll([FromQuery] int productId)
        {
            return await _billOfMaterialService.GetAll(productId);
        }
    }
}
