﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Stock;

namespace VErp.Services.Stock.Service.Stock

{
    public interface IStockService
    {
        /// <summary>
        /// Lấy danh sách kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<StockOutput>> GetList(string keyword, int page, int size);

        Task<PageData<StockOutput>> GetListByUserId(int userId,string keyword, int page, int size);

        Task<IList<SimpleStockInfo>> GetSimpleList();

        /// <summary>
        /// Thêm mới thông tin kho
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ServiceResult<int>> AddStock(StockModel req);

        /// <summary>
        /// Lấy thông tin của kho
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <returns></returns>
        Task<ServiceResult<StockOutput>> StockInfo(int stockId);

        /// <summary>
        /// Cập nhật thông tin kho
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<Enum> UpdateStock(int stockId, StockModel req);

        /// <summary>
        /// Xóa thông tin kho (đánh dấu xóa)
        /// </summary>
        /// <param name="stockId">Mã kho</param>
        /// <returns></returns>
        Task<Enum> DeleteStock(int stockId);

        Task<IList<StockWarning>> StockWarnings();

        Task<PageData<StockProductListOutput>> StockProducts(int stockId, string keyword, IList<int> productTypeIds, IList<int> productCateIds, IList<EnumWarningType> stockWarningTypeIds, int page, int size);

        Task<PageData<StockProductPackageDetail>> StockProductPackageDetails(int stockId, int productId, int page, int size);
        Task<PageData<LocationProductPackageOuput>> LocationProductPackageDetails(int stockId, int? locationId, int page, int size);
        Task<PageData<StockSumaryReportOutput>> StockSumaryReport(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, DateTime fromDate, DateTime toDate, int page, int size);
    }
}
