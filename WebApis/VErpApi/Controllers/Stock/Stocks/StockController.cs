using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
    }
}