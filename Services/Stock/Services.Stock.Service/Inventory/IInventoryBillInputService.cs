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
    public interface IInventoryBillInputService
    {
      
        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>      
        /// <param name="req"></param>        
        /// <returns></returns>
        Task<long> AddInventoryInput(InventoryInModel req);

        Task<long> AddInventoryInputDB(InventoryInModel req);

        /// <summary>
        /// Cập nhật thông tin phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>       
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdateInventoryInput(long inventoryId, InventoryInModel model);

      
        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>   
        /// <returns></returns>
        Task<bool> ApproveInventoryInput(long inventoryId);


      
        /// <summary>
        /// Xóa thông tin phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập xuất</param>
        /// <returns></returns>
        Task<bool> DeleteInventoryInput(long inventoryId);


        /// <summary>
        /// Lấy danh sách sản phẩm để nhập kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<ProductListOutput>> GetProductListForImport(string keyword, IList<int> stockIdList, int page = 1, int size = 20);
      
        Task<IList<CensoredInventoryInputProducts>> InputUpdateGetAffectedPackages(long inventoryId, long fromDate, long toDate, InventoryInModel req);

        Task<bool> ApprovedInputDataUpdate(long inventoryId, long fromDate, long toDate, ApprovedInputDataSubmitModel req);

    }
}
