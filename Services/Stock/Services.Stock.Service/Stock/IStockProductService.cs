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
    public interface IStockProductService
    {
       
        Task<PageData<StockOutput>> StockGetListByPermission(string keyword, int page, int size, Clause filters = null);

        Task<IList<StockWarning>> StockWarnings();

        Task<PageData<StockProductListOutput>> StockProducts(int stockId, string keyword, IList<int> productTypeIds, IList<int> productCateIds, IList<EnumWarningType> stockWarningTypeIds, int page, int size);

        Task<PageData<StockProductPackageDetail>> StockProductPackageDetails(int stockId, int productId, int page, int size);

        Task<PageData<LocationProductPackageOuput>> LocationProductPackageDetails(int stockId, int? locationId, IList<int> productTypeIds, IList<int> productCateIds, int page, int size);

        Task<PageData<StockProductQuantityWarning>> GetStockProductQuantityWarning(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, int page, int size);

        /// <summary>
        /// Báo cáo xuất, nhập tồn
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIds"></param>
        /// <param name="productTypeIds"></param>
        /// <param name="productCateIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<StockSumaryReportOutput>> StockSumaryReport(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, long fromDate, long toDate, string sortBy, bool asc, int page, int size);

        /// <summary>
        /// Báo cáo chi tiết nvl/sp
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="stockIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<StockProductDetailsReportOutput> StockProductDetailsReport(int productId, IList<int> stockIds, long fromDate, long toDate);

        /// <summary>
        /// Báo cáo tổng hợp NXT 2 DVT 2 DVT (SỐ LƯỢNG) - mẫu báo cáo 03
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<StockSumaryReportForm03Output>> StockSumaryReportForm03(string keyword, IList<int> stockIds, IList<int> productTypeIds, IList<int> productCateIds, long fromDate, long toDate, int page, int size);

        /// <summary>
        /// Nhật ký nhập xuất kho - mẫu báo cáo 04
        /// </summary>
        /// <param name="stockIds"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<PageData<StockSumaryReportForm04Output>> StockSumaryReportForm04(IList<int> stockIds, long beginTime, long endTime, int page, int size);
    }
}
