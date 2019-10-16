using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Stocks;

namespace VErp.Services.Stock.Service.Stocks

{
    public interface IStocksService
    {
        /// <summary>
        /// Lấy danh sách kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<StocksOutput>> GetList(string keyword, int page, int size);

        /// <summary>
        /// Thêm mới thông tin kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<int>> AddStocks(StocksModel req);

        /// <summary>
        /// Lấy thông tin của kho
        /// </summary>
        /// <param name="stocksId">Mã kho</param>
        /// <returns></returns>
        Task<ServiceResult<StocksOutput>> StocksInfo(int stocksId);

        /// <summary>
        /// Cập nhật thông tin kho
        /// </summary>
        /// <param name="stocksId">Mã kho</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<Enum> UpdateStocks(int stocksId, StocksModel req);

        /// <summary>
        /// Xóa thông tin kho (đánh dấu xóa)
        /// </summary>
        /// <param name="stocksId">Mã kho</param>
        /// <returns></returns>
        Task<Enum> DeleteStocks(int stocksId);
    }
}
