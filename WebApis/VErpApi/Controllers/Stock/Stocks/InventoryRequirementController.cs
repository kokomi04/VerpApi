using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.Stock;
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
        [VErpAction(EnumActionType.View)]
        [Route("inventorytype/{inventoryType}/Search")]
        public async Task<PageData<InventoryRequirementListModel>> GetListInventoryRequirements(
            [FromRoute] EnumInventoryType inventoryType,
            [FromQuery] string keyword,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string orderByFieldName,
            [FromQuery] bool asc,
            [FromQuery] bool hasInventory,
            [FromBody] Clause filters = null)
        {
            return await _inventoryRequirementService.GetListInventoryRequirements(inventoryType, keyword, page, size, orderByFieldName, asc, hasInventory, filters).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/code/{inventoryRequirementcode}")]
        public async Task<long> GetInventoryRequirementId([FromRoute] EnumInventoryType inventoryType, [FromRoute] string inventoryRequirementCode)
        {
            return await _inventoryRequirementService.GetInventoryRequirementId(inventoryType, inventoryRequirementCode);
        }

        [HttpGet]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<InventoryRequirementOutputModel> GetInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.GetInventoryRequirement(inventoryType, inventoryRequirementId);
        }
       
        [HttpPost]
        [Route("inventorytype/{inventoryType}")]
        public async Task<long> AddInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromBody] InventoryRequirementInputModel req)
        {
            return await _inventoryRequirementService.AddInventoryRequirement(inventoryType, req);
        }

        [HttpPut]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<long> UpdateInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId, [FromBody] InventoryRequirementInputModel req)
        {
            return await _inventoryRequirementService.UpdateInventoryRequirement(inventoryType, inventoryRequirementId, req);
        }

        [HttpDelete]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<bool> DeleteInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.DeleteInventoryRequirement(inventoryType, inventoryRequirementId);
        }

        [HttpPut]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}/accept")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> AcceptInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId, [FromBody] Dictionary<long, int> assignStocks)
        {
            return await _inventoryRequirementService.ConfirmInventoryRequirement(inventoryType, inventoryRequirementId, EnumInventoryRequirementStatus.Accepted, assignStocks);
        }

        [HttpPut]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}/reject")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> RejectInventoryRequirement([FromRoute]EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.ConfirmInventoryRequirement(inventoryType, inventoryRequirementId, EnumInventoryRequirementStatus.Rejected);
        }

        [HttpGet]
        [Route("inventorytype/{inventoryType}/inventoryrequirement")]
        public async Task<InventoryRequirementOutputModel> GetInventoryRequirement([FromRoute] EnumInventoryType inventoryType, [FromQuery] string productionOrderCode, [FromQuery] EnumInventoryRequirementType requirementType, [FromQuery] int productMaterialsConsumptionGroupId)
        {
            return await _inventoryRequirementService.GetInventoryRequirementByProductionOrderId(inventoryType, productionOrderCode, requirementType, productMaterialsConsumptionGroupId);
        }
    }
}
