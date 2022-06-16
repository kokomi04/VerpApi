using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Stock.Model.StockTake;
using VErp.Services.Stock.Service.StockTake;

namespace VErpApi.Controllers.Stock.StockTake
{
    [Route("api/stockTake")]

    public class StockTakeController : VErpBaseController
    {
        private readonly IStockTakeService _stockTakeService;
        public StockTakeController(IStockTakeService stockTakeService)
        {
            _stockTakeService = stockTakeService;
        }


        [HttpGet]
        [Route("{stockTakeId}")]
        public async Task<StockTakeModel> GetStockTake([FromRoute] long stockTakeId)
        {
            return await _stockTakeService.GetStockTake(stockTakeId);
        }

        [HttpPost]
        [Route("")]
        public async Task<StockTakeModel> CreateStockTake([FromBody] StockTakeModel model)
        {
            return await _stockTakeService.CreateStockTake(model);
        }

        [HttpPut]
        [Route("{stockTakeId}")]
        public async Task<StockTakeModel> UpdateStockTake([FromRoute] long stockTakeId, [FromBody] StockTakeModel model)
        {
            return await _stockTakeService.UpdateStockTake(stockTakeId, model);
        }


        [HttpDelete]
        [Route("{stockTakeId}")]
        public async Task<bool> DeleteStockTake([FromRoute] long stockTakeId)
        {
            return await _stockTakeService.DeleteStockTake(stockTakeId);
        }

        [HttpPut]
        [Route("approve/{stockTakeId}")]
        public async Task<bool> ApproveStockTake([FromRoute] long stockTakeId)
        {
            return await _stockTakeService.ApproveStockTake(stockTakeId);
        }
    }
}