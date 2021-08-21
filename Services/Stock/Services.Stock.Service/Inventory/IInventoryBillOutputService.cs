using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Service.Stock
{
    public interface IInventoryBillOutputService
    {
        Task<long> AddInventoryOutputDb(InventoryOutModel req);

        ///// <summary>
        ///// Thêm mới phiếu xuất kho
        ///// </summary>      
        ///// <param name="req"></param>
        ///// <param name="IsFreeStyle">IsFreeStyle = true: by pass unit conversion qty</param>
        ///// <returns></returns>
        Task<long> AddInventoryOutput(InventoryOutModel req);

        /// <summary>
        /// Cập nhật thông tin phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> UpdateInventoryOutput(long inventoryId, InventoryOutModel model);

        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>       
        /// <returns></returns>
        Task<bool> ApproveInventoryOutput(long inventoryId);


        /// <summary>
        /// Xóa thông tin phiếu xuất kho (đánh dấu xóa)
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập xuất</param>
        /// <returns></returns>
        Task<bool> DeleteInventoryOutput(long inventoryId);


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
    }
}
