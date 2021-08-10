using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Dictionary;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.StockTake;
using VErp.Services.Stock.Service.Dictionary;
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
        public async Task<StockTakeModel> UpdateStockTake([FromRoute] long stockTakedId, [FromBody] StockTakeModel model)
        {
            return await _stockTakeService.UpdateStockTake(stockTakedId, model);
        }


        [HttpDelete]
        [Route("{stockTakId}")]
        public async Task<bool> DeleteStockTake([FromRoute] long stockTakeId)
        {
            return await _stockTakeService.DeleteStockTake(stockTakeId);
        }

    }
}