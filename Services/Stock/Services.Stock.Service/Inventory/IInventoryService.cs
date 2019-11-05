using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Service.Invetory    
{
    /// <summary>
    /// I - Nhap xuat kho
    /// </summary>
    public interface IInventoryService
    {

        /// <summary>
        /// Lấy danh sách phiếu nhập / xuất kho
        /// </summary>
        /// <param name="keyword">Tìm kiếm trong Mã phiếu, mã SP, tên SP, tên người gủi/nhận, tên Obj liên quan RefObjectCode</param>
        /// <param name="stockId">Id kho</param>
        /// <param name="type">Loại typeId: 1 nhập ; 2 : xuất kho theo MasterEnum.EnumInventory</param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, EnumInventory type = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10);

        /// <summary>
        /// Thêm mới phiếu nhập / xuất kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> AddInventory(int currentUserId, InventoryInput req);

        /// <summary>
        /// Lấy thông tin của phiếu nhập xuất
        /// </summary>
        /// <param name="inventoryId">Mã vị trí</param>
        /// <returns></returns>
        Task<ServiceResult<InventoryOutput>> GetInventory(int inventoryId);

        /// <summary>
        /// Cập nhật thông tin phiếu nhập / xuất kho
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập / xuất kho</param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Enum> UpdateInventory(int inventoryId, int currentUserId, InventoryInput model);

        /// <summary>
        /// Xóa thông tin phiếu nhập / xuất kho (đánh dấu xóa)
        /// </summary>
        /// <param name="inventoryId">Mã phiếu nhập xuất</param>
        /// <returns></returns>
        Task<Enum> DeleteInventory(int inventoryId, int currentUserId);
    }
}
