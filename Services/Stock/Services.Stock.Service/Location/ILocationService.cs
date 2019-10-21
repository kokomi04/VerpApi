using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Location;

namespace VErp.Services.Stock.Service.Location    
{
    /// <summary>
    /// I - Vi trí trong kho hàng
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// Lấy danh sách vị trí
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <param name="keyword">Từ khóa cần tìm kiếm</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<LocationOutput>> GetList(int stockId,string keyword, int page, int size);

        /// <summary>
        /// Thêm mới thông tin vị trí
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<int>> AddLocation(LocationInput req);

        /// <summary>
        /// Lấy thông tin của vị trí
        /// </summary>
        /// <param name="locationId">Mã vị trí</param>
        /// <returns></returns>
        Task<ServiceResult<LocationOutput>> GetLocationInfo(int locationId);

        /// <summary>
        /// Cập nhật thông tin vị trí
        /// </summary>
        /// <param name="locationId">Mã vị trí</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<Enum> UpdateLocation(int locationId, LocationInput req);

        /// <summary>
        /// Xóa thông tin vị trí (đánh dấu xóa)
        /// </summary>
        /// <param name="locationId">Mã vị trí</param>
        /// <returns></returns>
        Task<Enum> DeleteLocation(int locationId);
    }
}
