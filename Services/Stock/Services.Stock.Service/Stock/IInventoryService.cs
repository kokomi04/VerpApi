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

        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>      
        /// <param name="req"></param>        
        /// <returns></returns>
        Task<long> AddInventoryInput(InventoryInModel req);


        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>      
        /// <param name="req"></param>
        /// <param name="IsFreeStyle">IsFreeStyle = true: by pass unit conversion qty</param>
        /// <returns></returns>
        Task<long> AddInventoryOutput(InventoryOutModel req);

        /// <summary>
        /// Cập nhật thông tin phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>       
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdateInventoryInput(long inventoryId, InventoryInModel model);

        /// <summary>
        /// Cập nhật thông tin phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdateInventoryOutput(long inventoryId, InventoryOutModel model);

        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>   
        /// <returns></returns>
        Task<bool> ApproveInventoryInput(long inventoryId);


        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>       
        /// <returns></returns>
        Task<bool> ApproveInventoryOutput(long inventoryId);

        /// <summary>
        /// Xóa thông tin phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập xuất</param>
        /// <returns></returns>
        Task<bool> DeleteInventoryInput(long inventoryId);

        /// <summary>
        /// Xóa thông tin phiếu xuất kho (đánh dấu xóa)
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập xuất</param>
        /// <returns></returns>
        Task<bool> DeleteInventoryOutput(long inventoryId);

        /// <summary>
        /// Lấy danh sách sản phẩm để nhập kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<ProductListOutput>> GetProductListForImport(string keyword, IList<int> stockIdList, int page = 1, int size = 20);

        /// <summary>
        /// Lấy danh sách sản phẩm để xuất kho
        /// </summary>
        /// <param name="keyword">Tìm kiếm trong Mã phiếu, mã SP, tên SP, tên người gủi/nhận, tên Obj liên quan RefObjectCode</param>
        /// <param name="stockIdList">Id kho</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<ProductListOutput>> GetProductListForExport(string keyword, IList<int> stockIdList, int page = 1, int size = 20);

        /// <summary>
        /// Lấy danh sách kiện để xuất kho
        /// </summary>
        /// <param name="productId">Id sản phẩm</param>
        /// <param name="stockIdList">List Id kho</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<PackageOutputModel>> GetPackageListForExport(int productId, IList<int> stockIdList, int page = 1, int size = 20);

        Task<IList<CensoredInventoryInputProducts>> InputUpdateGetAffectedPackages(long inventoryId, long fromDate, long toDate, InventoryInModel req);

        Task<bool> ApprovedInputDataUpdate(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req);


        CategoryNameModel FieldsForParse(EnumInventoryType inventoryTypeId);

        IAsyncEnumerable<InventoryDetailRowValue> ParseExcel(ImportExcelMapping mapping, Stream stream, EnumInventoryType inventoryTypeId);

    }
}
