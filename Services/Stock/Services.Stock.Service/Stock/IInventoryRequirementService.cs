using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Stock;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory.InventoryRequirement;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Stock
{
    public interface IInventoryRequirementService
    {
        Task<PageData<InventoryRequirementListModel>> GetListInventoryRequirements(EnumInventoryType inventoryType, string keyword, int page, int size, string orderByFieldName, bool asc, bool hasInventory, Clause filters = null);
        Task<long> GetInventoryRequirementId(EnumInventoryType inventoryType, string inventoryRequirementCode);
        Task<InventoryRequirementOutputModel> GetInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId);
        Task<long> AddInventoryRequirement(EnumInventoryType inventoryType, InventoryRequirementInputModel req);
        Task<long> UpdateInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId, InventoryRequirementInputModel req);
        Task<bool> DeleteInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId);

        Task<bool> ConfirmInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId, EnumInventoryRequirementStatus status, Dictionary<long, int> assignStocks = null);

        Task<InventoryRequirementOutputModel> GetInventoryRequirementByProductionOrderId(EnumInventoryType inventoryType, string productionOrderCode, EnumInventoryRequirementType requirementType, int productMaterialsConsumptionGroupId);
    }
}
