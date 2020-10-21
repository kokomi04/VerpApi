using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Stock;

namespace VErp.Services.Stock.Service.Stock

{
    public interface IStockService
    {

        /// <summary>
        /// Lấy toàn bộ danh sách kho, bao gồm cả những kho mà user đang đăng nhập không có quyền
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<StockOutput>> GetAll(string keyword, int page, int size, Clause filters);

        Task<PageData<StockOutput>> GetListByUserId(int userId, string keyword, int page, int size);

        Task<IList<SimpleStockInfo>> GetSimpleList();

        /// <summary>
        /// Thêm mới thông tin kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<int> AddStock(StockModel req);

        /// <summary>
        /// Lấy thông tin của kho
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <returns></returns>
        Task<StockOutput> StockInfo(int stockId);

        /// <summary>
        /// Cập nhật thông tin kho
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<bool> UpdateStock(int stockId, StockModel req);

        /// <summary>
        /// Xóa thông tin kho (đánh dấu xóa)
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <returns></returns>
        Task<bool> DeleteStock(int stockId);


    }
}
