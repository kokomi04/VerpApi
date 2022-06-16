using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionOrder;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductionOrderController : CrossServiceBaseController
    {
        private readonly IProductionOrderService _productionOrderService;
        public InternalProductionOrderController(IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
        }

        [HttpPut]
        [Route("status")]
        public async Task<bool> UpdateProductionOrderStatus([FromBody] ProductionOrderStatusDataModel data)
        {
            return await _productionOrderService.UpdateProductionOrderStatus(data);
        }
    }
}