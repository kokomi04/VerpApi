using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalStockController : CrossServiceBaseController
    {
        private readonly IStockService _stockService;
        public InternalStockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<ServiceResult<PageData<StockOutput>>> GetStocks([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockService.GetList(keyword, page, size);
        }

        [HttpGet]
        [Route("{stockId}")]
        public async Task<ServiceResult<StockOutput>> GetStocks([FromRoute] int stockId)
        {
            return await _stockService.StockInfo(stockId);
        }
    }
}