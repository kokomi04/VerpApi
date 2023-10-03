using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Stock;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory.InventoryRequirement;
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
            [FromQuery] bool? hasInventory,
            [FromBody] Clause filters = null)
        {
            return await _inventoryRequirementService.GetList(inventoryType, keyword, page, size, orderByFieldName, asc, hasInventory, filters).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/code")]
        public async Task<long> GetInventoryRequirementIdByCode([FromRoute] EnumInventoryType inventoryType, [FromQuery] string inventoryRequirementCode)
        {
            return await _inventoryRequirementService.GetIdByCode(inventoryType, inventoryRequirementCode);
        }

        [HttpGet]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<InventoryRequirementOutputModel> GetInventoryRequirement([FromRoute] EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.Info(inventoryType, inventoryRequirementId);
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("types/{inventoryTypeId}/GetByIds")]
        public async Task<IList<InventoryRequirementOutputModel>> GetByIds([FromRoute] EnumInventoryType inventoryTypeId, [FromBody] IList<long> inventoryRequirementIds)
        {
            return await _inventoryRequirementService.GetByIds(inventoryTypeId, inventoryRequirementIds);
        }

        [HttpPost]
        [Route("inventorytype/{inventoryType}")]
        public async Task<long> AddInventoryRequirement([FromRoute] EnumInventoryType inventoryType, [FromBody] InventoryRequirementInputModel req)
        {
            return await _inventoryRequirementService.Create(inventoryType, req);
        }

        [HttpPut]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<long> UpdateInventoryRequirement([FromRoute] EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId, [FromBody] InventoryRequirementInputModel req)
        {
            return await _inventoryRequirementService.Update(inventoryType, inventoryRequirementId, req);
        }

        [HttpDelete]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}")]
        public async Task<bool> DeleteInventoryRequirement([FromRoute] EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.Delete(inventoryType, inventoryRequirementId);
        }

        [HttpPut]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}/accept")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> AcceptInventoryRequirement([FromRoute] EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId, [FromBody] Dictionary<long, int> assignStocks)
        {
            return await _inventoryRequirementService.Confirm(inventoryType, inventoryRequirementId, EnumInventoryRequirementStatus.Accepted, assignStocks);
        }

        [HttpPut]
        [Route("inventorytype/{inventoryType}/inventoryrequirement/{inventoryRequirementId}/reject")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> Reject([FromRoute] EnumInventoryType inventoryType, [FromRoute] long inventoryRequirementId)
        {
            return await _inventoryRequirementService.Confirm(inventoryType, inventoryRequirementId, EnumInventoryRequirementStatus.Rejected);
        }

        [HttpGet]
        [Route("inventorytype/{inventoryType}/inventoryrequirement")]
        public async Task<InventoryRequirementOutputModel> GetByProductionOrderV1([FromRoute] EnumInventoryType inventoryType, [FromQuery] string productionOrderCode, [FromQuery] EnumInventoryRequirementType? requirementType, [FromQuery] int? productMaterialsConsumptionGroupId, [FromQuery] int? productionOrderMaterialSetId)
        {
            return (await _inventoryRequirementService.GetByProductionOrder(inventoryType, productionOrderCode, requirementType, productMaterialsConsumptionGroupId, productionOrderMaterialSetId)).FirstOrDefault();
        }

        [HttpGet]
        [Route("inventoryType/{inventoryType}/GetByProductionOrder")]
        public async Task<IList<InventoryRequirementOutputModel>> GetByProductionOrderV2([FromRoute] EnumInventoryType inventoryType, [FromQuery] string productionOrderCode, [FromQuery] EnumInventoryRequirementType? requirementType, [FromQuery] int? productMaterialsConsumptionGroupId, [FromQuery] int? productionOrderMaterialSetId)
        {
            return await _inventoryRequirementService.GetByProductionOrder(inventoryType, productionOrderCode, requirementType, productMaterialsConsumptionGroupId, productionOrderMaterialSetId);
        }
    }
}
