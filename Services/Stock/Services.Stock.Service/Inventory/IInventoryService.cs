using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;

namespace VErp.Services.Stock.Service.Invetory    
{
    /// <summary>
    /// I - Nhap xuat kho
    /// </summary>
    public interface IInventoryService
    {
      
        Task<PageData<InventoryOutput>> GetList(string keyword, int stockId = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10);

        /// <summary>
        /// Thêm mới thông tin vị trí
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<long>> AddInventory(InventoryInput req);

        ///// <summary>
        ///// Lấy thông tin của vị trí
        ///// </summary>
        ///// <param name="locationId">Mã vị trí</param>
        ///// <returns></returns>
        //Task<ServiceResult<LocationOutput>> GetLocationInfo(int locationId);

        ///// <summary>
        ///// Cập nhật thông tin vị trí
        ///// </summary>
        ///// <param name="locationId">Mã vị trí</param>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //Task<Enum> UpdateLocation(int locationId, LocationInput req);

        ///// <summary>
        ///// Xóa thông tin vị trí (đánh dấu xóa)
        ///// </summary>
        ///// <param name="locationId">Mã vị trí</param>
        ///// <returns></returns>
        //Task<Enum> DeleteLocation(int locationId);
    }
}
