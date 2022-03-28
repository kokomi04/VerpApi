using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;

namespace VErp.Services.Stock.Service.Stock
{
    public interface IInventoryBillOutputService
    {
        ObjectActivityLogModelBuilder<string> ImportedLogBuilder();


        Task<InventoryEntity> AddInventoryOutputDb(InventoryOutModel req);

        Task<long> AddInventoryOutput(InventoryOutModel req);
        
        Task<bool> UpdateInventoryOutput(long inventoryId, InventoryOutModel model);
       
        Task<bool> ApproveInventoryOutput(long inventoryId);

        Task ApproveInventoryOutputDb(InventoryEntity inventoryObj);


        Task<bool> DeleteInventoryOutput(long inventoryId);

        Task DeleteInventoryOutputDb(InventoryEntity inventoryObj);

        Task<bool> SentToCensor(long inventoryId);

        Task<bool> Reject(long inventoryId);
    }
}
