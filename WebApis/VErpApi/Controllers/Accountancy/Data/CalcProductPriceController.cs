using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Data
{
    [Route("api/accountancy/data/CalcProductPrice")]
    public class CalcProductPricePrivateController : CalcProductPriceControllerAbstract
    {
        public CalcProductPricePrivateController(ICalcProductPricePrivateService calcProductPriceService, ICalcPeriodPrivateService calcPeriodService, ICalcBillPrivateService calcBillService) 
            : base(calcProductPriceService, calcPeriodService, calcBillService)
        {

        }
    }

    [Route("api/accountancy/public/CalcProductPrice")]
    public class CalcProductPricePublicController : CalcProductPriceControllerAbstract
    {
        public CalcProductPricePublicController(ICalcProductPricePublicService calcProductPriceService, ICalcPeriodPublicService calcPeriodService, ICalcBillPublicService calcBillService)
            : base(calcProductPriceService, calcPeriodService, calcBillService)
        {

        }
    }



    public abstract class CalcProductPriceControllerAbstract : VErpBaseController
    {
        private readonly ICalcProductPriceServiceBase _calcProductPriceService;
        private readonly ICalcPeriodServiceBase _calcPeriodService;
        private readonly ICalcBillServiceBase _calcBillService;

        public CalcProductPriceControllerAbstract(ICalcProductPriceServiceBase calcProductPriceService, ICalcPeriodServiceBase calcPeriodService, ICalcBillServiceBase calcBillService)
        {
            _calcProductPriceService = calcProductPriceService;
            _calcPeriodService = calcPeriodService;
            _calcBillService = calcBillService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.Update)]
        [Route("CalcProductPriceTable")]
        public async Task<CalcProductPriceGetTableOutput> CalcProductPriceTable([FromBody] CalcProductPriceGetTableInput req)
        {
            return await _calcProductPriceService.CalcProductPriceTable(req).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("CalcProductPriceTablePeriods")]
        public async Task<PageData<CalcPeriodListModel>> CalcProductPriceTablePeriods([FromQuery] string keyword, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] int page, [FromQuery] int? size)
        {
            return await _calcPeriodService.GetList(EnumCalcPeriodType.CalcProductPrice, keyword, fromDate, toDate, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("CalcProductPriceTablePeriods/{calcPeriodId}")]
        public async Task<CalcPeriodView<CalcProductPriceGetTableInput, CalcProductPriceGetTableOutput>> CalcProductPriceTablePeriodsInfo([FromRoute] long calcPeriodId)
        {
            return await _calcPeriodService.CalcPeriodInfo<CalcProductPriceGetTableInput, CalcProductPriceGetTableOutput>(EnumCalcPeriodType.CalcProductPrice, calcPeriodId).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("CalcProductPriceTablePeriods/{calcPeriodId}")]
        public async Task<bool> CalcProductPriceTablePeriodsDelete([FromRoute] long calcPeriodId)
        {
            return await _calcPeriodService.Delete(EnumCalcPeriodType.CalcProductPrice, calcPeriodId).ConfigureAwait(true);
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
        public async Task<IList<NonCamelCaseDictionary>> GetWeightedAverageProductPrice([FromBody] CalcProductPriceInput req)
        {
            return await _calcProductPriceService.GetWeightedAverageProductPrice(req).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        [Route("GetProductPriceBuyLastest")]
        public async Task<IList<NonCamelCaseDictionary>> GetProductPriceBuyLastest([FromBody] CalcProductPriceInput req)
        {
            return await _calcProductPriceService.GetProductPriceBuyLastest(req).ConfigureAwait(true);
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
            return await _calcPeriodService.GetList(EnumCalcPeriodType.CalcProfitAndLoss, keyword, fromDate, toDate, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("CalcProfitAndLossPeriods/{calcPeriodId}")]
        public async Task<CalcPeriodView<CalcProfitAndLossInput, CalcProfitAndLossTableOutput>> CalcProfitAndLossPeriodInfo([FromRoute] long calcPeriodId)
        {
            return await _calcPeriodService.CalcPeriodInfo<CalcProfitAndLossInput, CalcProfitAndLossTableOutput>(EnumCalcPeriodType.CalcProfitAndLoss, calcPeriodId).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("CalcProfitAndLossPeriods/{calcPeriodId}")]
        public async Task<bool> CalcProfitAndLossPeriodDelete([FromRoute] long calcPeriodId)
        {
            return await _calcPeriodService.Delete(EnumCalcPeriodType.CalcProfitAndLoss, calcPeriodId).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("CalcFixExchangeRateByOrder")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string tk)
        {
            return await _calcBillService.CalcFixExchangeRateByOrder(fromDate, toDate, currency, tk);
        }


        [HttpGet]
        [Route("CheckExistedFixExchangeRateByOrder")]
        public async Task<bool> CheckExistedFixExchangeRate([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string tk)
        {
            return await _calcBillService.CheckExistedFixExchangeRateByOrder(fromDate, toDate, currency, tk);
        }


        [HttpDelete]
        [Route("DeletedFixExchangeRateByOrder")]
        public async Task<bool> DeletedFixExchangeRateByOrder([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string tk)
        {
            return await _calcBillService.DeletedFixExchangeRateByOrder(fromDate, toDate, currency, tk);
        }




        [HttpGet]
        [Route("CalcFixExchangeRateByLoanCovenant")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRateByLoanCovenant([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string tk)
        {
            return await _calcBillService.CalcFixExchangeRateByLoanCovenant(fromDate, toDate, currency, tk);
        }


        [HttpGet]
        [Route("CheckExistedFixExchangeRateByLoanCovenant")]
        public async Task<bool> CheckExistedFixExchangeRateByLoanCovenant([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string tk)
        {
            return await _calcBillService.CheckExistedFixExchangeRateByLoanCovenant(fromDate, toDate, currency, tk);
        }


        [HttpDelete]
        [Route("DeleteFixExchangeRateByLoanCovenant")]
        public async Task<bool> DeleteFixExchangeRateByLoanCovenant([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string tk)
        {
            return await _calcBillService.DeleteFixExchangeRateByLoanCovenant(fromDate, toDate, currency, tk);
            
        }
    }
}
