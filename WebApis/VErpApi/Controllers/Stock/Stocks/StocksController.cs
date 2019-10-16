using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Stocks;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Stocks;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/stocks")]

    public class StocksController : VErpBaseController
    {
        private readonly IStocksService _stocksService;
        public StocksController(IStocksService stocksService
            )
        {
            _stocksService = stocksService;
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
        public async Task<ApiResponse<PageData<StocksOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stocksService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Thêm mới kho sản phẩm
        /// </summary>
        /// <param name="stocks"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddStocks([FromBody] StocksModel stocks)
        {
            return await _stocksService.AddStocks(stocks);
        }

        /// <summary>
        /// Lấy thông tin kho sản phẩm
        /// </summary>
        /// <param name="stocksId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{stocksId}")]
        public async Task<ApiResponse<StocksOutput>> GetStocks([FromRoute] int stocksId)
        {
            return await _stocksService.StocksInfo(stocksId);
        }

        /// <summary>
        /// Cập nhật thông tin kho sản phẩm
        /// </summary>
        /// <param name="stocksId"></param>
        /// <param name="stocks"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{stocksId}")]
        public async Task<ApiResponse> UpdateStocks([FromRoute] int stocksId, [FromBody] StocksModel stocks)
        {
            return await _stocksService.UpdateStocks(stocksId, stocks);
        }

        /// <summary>
        /// Xóa thông tin kho sản phẩm
        /// </summary>
        /// <param name="stocksId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{stocksId}")]
        public async Task<ApiResponse> Delete([FromRoute] int stocksId)
        {
            return await _stocksService.DeleteStocks(stocksId);
        }
    }
}