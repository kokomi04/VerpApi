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
    [Route("api/stockTakePeriod")]

    public class StockTakePeriodController : VErpBaseController
    {
        private readonly IStockTakePeriodService _stockTakePeriodService;
        public StockTakePeriodController(IStockTakePeriodService stockTakePeriodService)
        {
            _stockTakePeriodService = stockTakePeriodService;
        }


      
        [HttpGet]
        [Route("")]
        public async Task<PageData<StockTakePeriotListModel>> GetStockTakePeriods([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stockTakePeriodService.GetStockTakePeriods(keyword, page, size);
        }

     
        [HttpGet]
        [Route("{stockTakePeriodId}")]
        public async Task<StockTakePeriotModel> GetStockTakePeriod([FromRoute] long stockTakePeriodId)
        {
            return await _stockTakePeriodService.GetStockTakePeriod(stockTakePeriodId);
        }

        [HttpPost]
        [Route("")]
        public async Task<StockTakePeriotModel> CreateStockTakePeriod([FromBody] StockTakePeriotModel model)
        {
            return await _stockTakePeriodService.CreateStockTakePeriod(model);
        }

        [HttpPut]
        [Route("{stockTakePeriodId}")]
        public async Task<StockTakePeriotModel> UpdateStockTakePeriod([FromRoute] long stockTakePeriodId, [FromBody] StockTakePeriotModel model)
        {
            return await _stockTakePeriodService.UpdateStockTakePeriod(stockTakePeriodId, model);
        }

     
        [HttpDelete]
        [Route("{stockTakePeriodId}")]
        public async Task<bool> DeleteStockTakePeriod([FromRoute] long stockTakePeriodId)
        {
            return await _stockTakePeriodService.DeleteStockTakePeriod(stockTakePeriodId);
        }

    }
}