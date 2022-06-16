using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductMaterialsConsumptionController : CrossServiceBaseController
    {
        private readonly IProductMaterialsConsumptionService _productMaterialsConsumptionService;
        public InternalProductMaterialsConsumptionController(IProductMaterialsConsumptionService productMaterialsConsumptionService)
        {
            _productMaterialsConsumptionService = productMaterialsConsumptionService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumption([FromBody] int[] productIds)
        {
            return await _productMaterialsConsumptionService.GetProductMaterialsConsumption(productIds);
        }
    }
}