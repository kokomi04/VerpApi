using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory.InventoryRequirement;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Inventory
{
    [Route("api/inventoryRequirement")]
    public class InventoryRequirementController : VErpBaseController
    {
        private readonly IInventoryRequirementService _inventoryRequirementService;

        public InventoryRequirementController(IInventoryRequirementService inventoryRequirementService)
        {
            _inventoryRequirementService = inventoryRequirementService;
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("inventorytype/{inventoryType}/Search")]
        public async Task<PageData<InventoryRequirementModel>> GetListInventoryRequirements([FromRoute]EnumInventoryType inventoryType, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _inventoryRequirementService.GetListInventoryRequirements(inventoryType, keyword, page, size, orderByFieldName, asc, filters).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<InventoryRequirementModel> GetInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.GetInventoryRequirement(inventoryType, inventoryRequirementId);
        }

       
        [HttpPost]
        [Route("inventorytype/{inventoryType}")]
        public async Task<InventoryRequirementModel> AddInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromBody] InventoryRequirementModel req)
        {
            return await _inventoryRequirementService.AddInventoryRequirement(inventoryType, req);

        }

        [HttpPut]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<InventoryRequirementModel> UpdateInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId, [FromBody] InventoryRequirementModel req)
        {
            return await _inventoryRequirementService.UpdateInventoryRequirement(inventoryType, inventoryRequirementId, req);
        }

        [HttpDelete]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<bool> DeleteInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.DeleteInventoryRequirement(inventoryType, inventoryRequirementId);
        }
    }
}
