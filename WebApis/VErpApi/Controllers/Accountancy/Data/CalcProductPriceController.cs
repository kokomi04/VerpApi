using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;
using System.IO;

namespace VErpApi.Controllers.Accountancy.Data
{
    [Route("api/accountancy/data/CalcProductPrice")]
    public class CalcProductPriceController : VErpBaseController
    {
        private readonly ICalcProductPriceService _calcProductPriceService;

        public CalcProductPriceController(ICalcProductPriceService calcProductPriceService)
        {
            _calcProductPriceService = calcProductPriceService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.Update)]
        [Route("CalcProductPriceTable")]
        public async Task<CalcProductPriceGetTableOutput> CalcProductPriceTable([FromBody] CalcProductPriceGetTableInput req)
        {
            return await _calcProductPriceService.CalcProductPriceTable(req).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.Update)]
        [Route("CalcProductOutputPrice")]
        public async Task<CalcProductOutputPriceModel> CalcProductOutputPrice([FromBody] CalcProductOutputPriceInput req)
        {
            return await _calcProductPriceService.CalcProductOutputPrice(req).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        [Route("GetWeightedAverageProductPrice")]
        public async Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice([FromBody] CalcProductPriceWeightedAverageInput req)
        {
            return await _calcProductPriceService.GetWeightedAverageProductPrice(req).ConfigureAwait(true);
        }


        [HttpPost]
        [VErpAction(EnumActionType.Update)]
        [Route("CalcProfitAndLoss")]
        public async Task<CalcProfitAndLossTableOutput> CalcProfitAndLoss([FromBody] CalcProfitAndLossInput req)
        {
            return await _calcProductPriceService.CalcProfitAndLoss(req).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("CalcProfitAndLossPeriods")]
        public async Task<PageData<CalcPeriodListModel>> CalcProfitAndLossPeriods([FromQuery] string keyword, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] int page, [FromQuery] int? size)
        {
            return await _calcProductPriceService.CalcProfitAndLossPeriods(keyword, fromDate, toDate, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("CalcProfitAndLossPeriods/{calcPeriodId}")]
        public async Task<CalcProfitAndLossView> CalcProfitAndLossPeriodInfo([FromRoute] long calcPeriodId)
        {
            return await _calcProductPriceService.CalcProfitAndLossPeriodInfo(calcPeriodId).ConfigureAwait(true);
        }

    }
}
