using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionHandover;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductionHandoverController : CrossServiceBaseController
    {
        private readonly IProductionHandoverService _productionHandoverService;
        private readonly IMaterialAllocationService _materialAllocationService;

        public InternalProductionHandoverController(IProductionHandoverService productionHandoverService, IMaterialAllocationService materialAllocationService)
        {
            _productionHandoverService = productionHandoverService;
            _materialAllocationService = materialAllocationService;
        }

        [HttpPut]
        [Route("status")]
        public async Task<bool> ChangeAssignedProgressStatus([FromBody] ProductionOrderStatusDataModel data)
        {
            return await _productionHandoverService.ChangeAssignedProgressStatus(data.ProductionOrderCode, data.InventoryCode, data.Inventories);
        }

        [HttpPut]
        [Route("ignore-allocation")]
        public async Task<bool> UpdateIgnoreAllocation([FromBody] string[] productionOrderCodes)
        {
            return await _materialAllocationService.UpdateIgnoreAllocation(productionOrderCodes);
        }
    }
}