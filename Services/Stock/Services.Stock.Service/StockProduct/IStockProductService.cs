using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Service.StockProduct
{
    public interface IStockProductService
    {
        /// <summary>
        /// Lấy danh sách StockProduct
        /// </summary>        
        /// <param name="stockId">Id kho</param>
        /// <param name="productId">Id VTHHSP</param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<VErp.Infrastructure.EF.StockDB.StockProduct>> GetList(int stockId = 0, int productId = 0, DateTime? beginTime = null, DateTime? endTime = null, int page = 1, int size = 10);

        /// <summary>
        /// Lấy thông tin StockProduct
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <param name="productId">Mã VTHHSP</param>
        /// <returns></returns>
        Task<ServiceResult<VErp.Infrastructure.EF.StockDB.StockProduct>> GetStockProduct(int stockId = 0, int productId = 0);

        ///// <summary>
        ///// Lưu StockProduct (thêm + sửa)
        ///// </summary>
        ///// <param name="model">StockProduct model</param>
        ///// <returns></returns>
        //Task<Enum> SaveStockProduct(int currentUserId, VErp.Infrastructure.EF.StockDB.StockProduct model);
    }
}
