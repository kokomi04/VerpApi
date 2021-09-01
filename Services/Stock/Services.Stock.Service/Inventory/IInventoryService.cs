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
    /// <summary>
    /// I - Nhap xuat kho
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Lấy danh sách phiếu nhập / xuất kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="customerId"></param>
        /// <param name="accountancyAccountNumber"></param>
        /// <param name="stockId"></param>
        /// <param name="isApproved"></param>
        /// <param name="type"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="isExistedInputBill"></param>
        /// <param name="mappingFunctionKeys"></param>
        /// <param name="sortBy"></param>
        /// <param name="asc"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<InventoryOutput>> GetList(string keyword, int? customerId, IList<int> productIds, string accountancyAccountNumber, int stockId = 0, bool? isApproved = null, EnumInventoryType? type = null, long? beginTime = 0, long? endTime = 0, bool? isExistedInputBill = null, IList<string> mappingFunctionKeys = null, string sortBy = "date", bool asc = false, int page = 1, int size = 10);


        /// <summary>
        /// Lấy thông tin của phiếu nhập xuất
        /// </summary>
        /// <param name="inventoryId">Mã phiếu</param>
        /// <returns></returns>
        Task<InventoryOutput> InventoryInfo(long inventoryId, IList<string> mappingFunctionKeys = null);


        Task<(Stream stream, string fileName, string contentType)> InventoryInfoExport(long inventoryId, IList<string> mappingFunctionKeys = null);

        CategoryNameModel GetInventoryDetailFieldDataForMapping();

        Task<long> InventoryImport(ImportExcelMapping mapping, Stream stream, InventoryOpeningBalanceModel model);
              
        
        CategoryNameModel FieldsForParse(EnumInventoryType inventoryTypeId);

        IAsyncEnumerable<InventoryDetailRowValue> ParseExcel(ImportExcelMapping mapping, Stream stream, EnumInventoryType inventoryTypeId);

    }
}
