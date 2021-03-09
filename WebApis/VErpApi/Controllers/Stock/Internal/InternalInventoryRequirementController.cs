using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Stock;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Stock.Model.Inventory.InventoryRequirement;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalInventoryRequirementController: CrossServiceBaseController
    {
        private readonly IInventoryRequirementService _inventoryRequirementService;

        public InternalInventoryRequirementController(IInventoryRequirementService inventoryRequirementService)
        {
            _inventoryRequirementService = inventoryRequirementService;
        }

        [HttpPost]
        [Route("{inventoryType}")]
        public async Task<long> AddInventoryRequirementWithConfirm([FromRoute] EnumInventoryType inventoryType, [FromBody] InventoryRequirementInputModel req)
        {
            return await _inventoryRequirementService.AddInventoryRequirement(inventoryType, req);
        }
    }
}
