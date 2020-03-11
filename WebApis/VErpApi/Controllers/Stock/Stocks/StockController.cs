using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Stocks
{
    [Route("api/stocks")]

    public class StockController : VErpBaseController
    {
        private readonly IStockService _stockService;
        public StockController(IStockService stockService
            )
        {
            _stockService = stockService;
        }


        /// <summary>
        /// Tìm kiếm tất cả danh sách kho (Bao gồm cả những kho mà user không có quyền)
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAll")]
        public async Task<ApiResponse<PageData<StockOutput>>> GetAll([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.GetAll(keyword, page, size);
        }

        /// <summary>
        /// Tìm kiếm kho sản phẩm
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<StockOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Lấy danh sách kho mà người dùng đc phân quyền quản lý
        /// </summary>
        /// <param name="keyword">Từ khoá tìm kiếm</param>
        /// <param name="page">Trang</param>
        /// <param name="size">Sổ bản ghi 1 trang</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetListByUserId")]
        public async Task<ApiResponse<PageData<StockOutput>>> GetListByUserId([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            var currentUserId = UserId;
            return await _stockService.GetListByUserId(currentUserId, keyword, page, size);
        }

        /// <summary>
        /// Lấy danh sách kho
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("SimpleList")]
        public async Task<ApiResponse<IList<SimpleStockInfo>>> SimpleList()
        {
            return (await _stockService.GetSimpleList()).ToList();
        }

        /// <summary>
        /// Thêm mới kho sản phẩm
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddStocks([FromBody] StockModel stock)
        {
            return await _stockService.AddStock(stock);
        }

        /// <summary>
        /// Lấy thông tin kho sản phẩm
        /// </summary>
        /// <param name="stockId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{stockId}")]
        public async Task<ApiResponse<StockOutput>> GetStocks([FromRoute] int stockId)
        {
            return await _stockService.StockInfo(stockId);
        }

        /// <summary>
        /// Cập nhật thông tin kho sản phẩm
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="stock"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{stockId}")]
        public async Task<ApiResponse> UpdateStock([FromRoute] int stockId, [FromBody] StockModel stock)
        {
            return await _stockService.UpdateStock(stockId, stock);
        }

        /// <summary>
        /// Xóa thông tin kho sản phẩm
        /// </summary>
        /// <param name="stockId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{stockId}")]
        public async Task<ApiResponse> Delete([FromRoute] int stockId)
        {
            return await _stockService.DeleteStock(stockId);
        }

        /// <summary>
        /// Lấy toàn bộ kho và các cảnh báo của kho
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("StockWarnings")]
        public async Task<ApiResponse<IList<StockWarning>>> StockWarnings()
        {
            return (await _stockService.StockWarnings()).ToList();
        }

        /// <summary>
        /// Lấy danh sách sản phẩm trong kho
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="keyword"></param>
        /// <param name="productTypeIds"></param>
        /// <param name="productCateIds"></param>
        /// <param name="stockWarningTypeIds"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{stockId}/StockProducts")]
        public async Task<ApiResponse<PageData<StockProductListOutput>>> StockProducts([FromRoute] int stockId, [FromQuery] string keyword, [FromQuery] IList<int> productTypeIds, [FromQuery] IList<int> productCateIds, [FromQuery] IList<EnumWarningType> stockWarningTypeIds, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.StockProducts(stockId, keyword, productTypeIds, productCateIds, stockWarningTypeIds, page, size);
        }

        /// <summary>
        /// Lấy danh sách kiện theo sản phẩm trong kho
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="productId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{stockId}/StockProducts/{productId}")]
        public async Task<ApiResponse<PageData<StockProductPackageDetail>>> StockProductPackageDetails([FromRoute] int stockId, [FromRoute] int productId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.StockProductPackageDetails(stockId, productId, page, size);
        }

        /// <summary>
        /// Lấy danh sách kiện theo vị trí trong kho
        /// </summary>
        /// <param name="stockId"></param>
        /// <param name="locationId"></param>
        /// <param name="productTypeIds"></param>
        /// <param name="productCateIds"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{stockId}/Packages")]
        public async Task<ApiResponse<PageData<LocationProductPackageOuput>>> LocationProductPackageDetails([FromRoute] int stockId, [FromQuery] int? locationId, [FromQuery] IList<int> productTypeIds, [FromQuery] IList<int> productCateIds, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.LocationProductPackageDetails(stockId, locationId, productTypeIds, productCateIds, page, size);
        }

        /// <summary>
        /// Lấy danh sách sản phẩm có số lượng tồn kho và có cảnh báo tồn mix max trong kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIds"></param>
        /// <param name="productTypeIds"></param>
        /// <param name="productCateIds"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetStockProductQuantityWarning")]
        public async Task<ApiResponse<PageData<StockProductQuantityWarning>>> GetStockProductQuantityWarning([FromQuery] string keyword, [FromQuery] IList<int> stockIds, [FromQuery] IList<int> productTypeIds, [FromQuery] IList<int> productCateIds, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.GetStockProductQuantityWarning(keyword, stockIds, productTypeIds, productCateIds, page, size);
        }

        /// <summary>
        /// Báo cáo xuất, nhập tồn
        /// </summary>
        /// <param name="stockIds"></param>
        /// <param name="productTypeIds"></param>
        /// <param name="productCateIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="keyword">Từ khoá tìm kiếm: ProductCode | ProductName</param>
        /// <param name="sortBy">sort by column (default: date) </param>
        /// <param name="asc">true/false (default: false. It mean sort desc)</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StockSumaryReport")]
        public async Task<ApiResponse<PageData<StockSumaryReportOutput>>> StockSumaryReport([FromQuery] IList<int> stockIds, [FromQuery] IList<int> productTypeIds, [FromQuery] IList<int> productCateIds, [FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] string keyword, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.StockSumaryReport(keyword, stockIds, productTypeIds, productCateIds, fromDate, toDate, sortBy, asc, page, size);
        }

        /// <summary>
        /// Báo cáo chi tiết nhập xuất VTHHSP trong kỳ
        /// </summary>
        /// <param name="productId">Id VTHHSP</param>
        /// <param name="stockIds">List id of stock</param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StockProductDetailsReport")]
        public async Task<ApiResponse<ServiceResult<StockProductDetailsReportOutput>>> StockProductDetailsReport([FromQuery] int productId, [FromQuery] IList<int> stockIds, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _stockService.StockProductDetailsReport(productId, stockIds, fromDate, toDate);
        }

        /// <summary>
        /// Báo cáo tổng hợp NXT 2 DVT 2 DVT (SỐ LƯỢNG) - - Mẫu báo cáo kho 03
        /// </summary>
        /// <param name="stockIds"></param>
        /// <param name="keyword"></param>
        /// <param name="productTypeIds"></param>
        /// <param name="productCateIds"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StockSumaryReportForm03")]
        public async Task<ApiResponse<PageData<StockSumaryReportForm03Output>>> StockSumaryReportForm03([FromQuery] IList<int> stockIds, [FromQuery] string keyword, [FromQuery] IList<int> productTypeIds, [FromQuery] IList<int> productCateIds, [FromQuery] long fromDate, [FromQuery] long toDate,  [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.StockSumaryReportForm03(keyword, stockIds, productTypeIds, productCateIds, fromDate, toDate, page, size);
        }

        /// <summary>
        /// Báo cáo nhật ký nhập xuất kho - Mẫu báo cáo kho 04
        /// </summary>
        /// <param name="stockIds">Danh sách id kho cần báo cáo</param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("StockSumaryReportForm04")]
        public async Task<ApiResponse<PageData<StockSumaryReportForm04Output>>> StockSumaryReportForm04([FromQuery] IList<int> stockIds, [FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.StockSumaryReportForm04(stockIds, fromDate, toDate, page, size);
        }
    }
}