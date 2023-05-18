using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Service.Stock
{
    public interface IInventoryService
    {
        Task<PageData<InventoryListOutput>> GetList(string keyword, int? customerId, IList<int> productIds, int stockId = 0, int? inventoryStatusId = null, EnumInventoryType? type = null, long? beginTime = 0, long? endTime = 0, bool? isExistedInputBill = null, string sortBy = "date", bool asc = false, int page = 1, int size = 10, int? inventoryActionId = null, Clause filters = null);

        Task<InventoryOutput> InventoryInfo(long inventoryId);

        Task<(Stream stream, string fileName, string contentType)> InventoryInfoExport(long inventoryId);

        Task<CategoryNameModel> InputFieldsForMapping();

        CategoryNameModel OutputFieldsForMapping();

        Task<bool> InventoryInputImport(ImportExcelMapping mapping, Stream stream);

        Task<bool> InventoryOutImport(ImportExcelMapping mapping, Stream stream);

        CategoryNameModel OutFieldsForParse();

        Task<CategoryNameModel> InputFieldsForParse();

        IAsyncEnumerable<InvInputDetailRowValue> InputParseExcel(ImportExcelMapping mapping, Stream stream, int stockId);

        IAsyncEnumerable<InvOutDetailRowValue> OutParseExcel(ImportExcelMapping mapping, Stream stream, int stockId);

        Task<bool> SendMailNotifyCensor(long inventoryId, string mailCode, string[] mailTo);


        Task ProductionOrderInventory(ProductionOrderStatusInventorySumaryMessage msg);

    }
}
