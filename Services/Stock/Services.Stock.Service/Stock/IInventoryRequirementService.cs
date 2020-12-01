using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
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
        Task<PageData<InventoryRequirementModel>> GetListInventoryRequirements(EnumInventoryType inventoryType, string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null);
        Task<InventoryRequirementModel> GetInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId);
        Task<InventoryRequirementModel> AddInventoryRequirement(EnumInventoryType inventoryType, InventoryRequirementModel req);
        Task<InventoryRequirementModel> UpdateInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId, InventoryRequirementModel req);
        Task<bool> DeleteInventoryRequirement(EnumInventoryType inventoryType, long inventoryRequirementId);
    }
}
