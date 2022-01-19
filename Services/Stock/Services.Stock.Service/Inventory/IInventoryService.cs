using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Stock
{  
    public interface IInventoryService
    {
        Task<PageData<InventoryOutput>> GetList(string keyword, int? customerId, IList<int> productIds, int stockId = 0, int? inventoryStatusId = null, EnumInventoryType? type = null, long? beginTime = 0, long? endTime = 0, bool? isExistedInputBill = null, string sortBy = "date", bool asc = false, int page = 1, int size = 10, int? inventoryActionId = null);

        Task<InventoryOutput> InventoryInfo(long inventoryId);

        Task<(Stream stream, string fileName, string contentType)> InventoryInfoExport(long inventoryId);

        CategoryNameModel GetInventoryDetailFieldDataForMapping();

        Task<long> InventoryImport(ImportExcelMapping mapping, Stream stream, InventoryOpeningBalanceModel model);
                      
        CategoryNameModel FieldsForParse(EnumInventoryType inventoryTypeId);

        IAsyncEnumerable<InventoryDetailRowValue> ParseExcel(ImportExcelMapping mapping, Stream stream, EnumInventoryType inventoryTypeId);

        Task<bool> SendMailNotifyCensor(long inventoryId, string mailCode, string[] mailTo);      
        
    }
}
