using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using InventoryEntity = VErp.Infrastructure.EF.StockDB.Inventory;


namespace VErp.Services.Stock.Service.Stock
{
    public interface IInventoryBillInputService
    {

        ObjectActivityLogModelBuilder<string> ImportedLogBuilder();


        Task<long> AddInventoryInput(InventoryInModel req);

        Task<InventoryEntity> AddInventoryInputDB(InventoryInModel req, bool validatePackageInfo);

        Task<bool> UpdateInventoryInput(long inventoryId, InventoryInModel model);

        Task<bool> ApproveInventoryInput(long inventoryId);

        Task ApproveInventoryInputDb(InventoryEntity inventoryObj, GenerateCodeConfigData genCodeConfig);


        Task<bool> DeleteInventoryInput(long inventoryId);

        Task DeleteInventoryInputDb(InventoryEntity inventoryObj);


        Task<PageData<ProductListOutput>> GetProductListForImport(string keyword, IList<int> productCateIds, IList<int> stockIdList, int page = 1, int size = 20);

        Task<IList<CensoredInventoryInputProducts>> InputUpdateGetAffectedPackages(long inventoryId, long fromDate, long toDate, InventoryInModel req);

        Task<bool> ApprovedInputDataUpdate(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req);

        Task<(HashSet<long> affectedInventoryIds, bool isDeleted)> ApprovedInputDataUpdateDb(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req, GenerateCodeConfigData genCodeConfig);

        Task<bool> SentToCensor(long inventoryId);

        Task<bool> Reject(long inventoryId);
    }
}
