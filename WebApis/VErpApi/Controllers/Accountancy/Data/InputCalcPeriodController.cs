using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Data
{

    [Route("api/accountancy/data/CalcPeriod")]
    public class InputCalcPeriodPrivateController : InputCalcPeriodControllerAbstract
    {
        public InputCalcPeriodPrivateController(ICalcBillPrivateService calcBillService) : base(calcBillService)
        {

        }
    }

    [Route("api/accountancy/public/CalcPeriod")]
    public class InputCalcPeriodPublicController : InputCalcPeriodControllerAbstract
    {
        public InputCalcPeriodPublicController(ICalcBillPublicService calcBillService) : base(calcBillService)
        {
        }
    }


    public abstract class InputCalcPeriodControllerAbstract : VErpBaseController
    {
        private readonly ICalcBillServiceBase _calcBillService;

        public InputCalcPeriodControllerAbstract(ICalcBillServiceBase calcBillService)
        {
            _calcBillService = calcBillService;
        }


        [HttpGet]
        [Route("CalcFixExchangeRate")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate([FromQuery] long toDate, [FromQuery] int currency, [FromQuery] int exchangeRate, [FromQuery] string accoutantNumber)
        {
            return await _calcBillService.CalcFixExchangeRate(toDate, currency, exchangeRate, accoutantNumber);
        }

        [HttpGet]
        [Route("FixExchangeRateDetail")]
        public async Task<DataResultModel> FixExchangeRateDetail([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string accountNumber, [FromQuery] string partnerId)
        {
            return await _calcBillService.FixExchangeRateDetail(fromDate, toDate, currency, accountNumber, partnerId);
        }

        [HttpGet]
        [Route("CalcCostTransfer")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcCostTransfer([FromQuery] long toDate, [FromQuery] EnumCostTransfer type, [FromQuery] bool byDepartment,
            [FromQuery] bool byCustomer, [FromQuery] bool byFixedAsset, [FromQuery] bool byExpenseItem, [FromQuery] bool byFactory, [FromQuery] bool byProduct, [FromQuery] bool byStock)
        {
            return await _calcBillService.CalcCostTransfer(toDate, type, byDepartment, byCustomer, byFixedAsset, byExpenseItem, byFactory, byProduct, byStock);
        }

        [HttpGet]
        [Route("CalcCostTransferDetail")]
        public async Task<DataResultModel> CalcCostTransferDetail([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] EnumCostTransfer type,
            [FromQuery] bool byDepartment, [FromQuery] bool byCustomer, [FromQuery] bool byFixedAsset, [FromQuery] bool byExpenseItem, [FromQuery] bool byFactory, [FromQuery] bool byProduct, [FromQuery] bool byStock,
            [FromQuery] int? department, [FromQuery] string customer, [FromQuery] int? fixedAsset, [FromQuery] int? expenseItem, [FromQuery] int? factory, [FromQuery] int? product, [FromQuery] int? stock)
        {
            return await _calcBillService.CalcCostTransferDetail(fromDate, toDate, type,
                byDepartment, byCustomer, byFixedAsset, byExpenseItem, byFactory, byProduct, byStock,
                department, customer, fixedAsset, expenseItem, factory, product, stock);
        }

        [HttpGet]
        [Route("CostTransferType")]
        public ICollection<CostTransferTypeModel> GetCostTransferTypes()
        {
            return _calcBillService.GetCostTransferTypes();
        }

        [HttpGet]
        [Route("CheckExistedFixExchangeRate")]
        public async Task<bool> CheckExistedFixExchangeRate([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string accoutantNumber)
        {
            return await _calcBillService.CheckExistedFixExchangeRate(fromDate, toDate, currency, accoutantNumber);
        }

        [HttpDelete]
        [Route("DeletedFixExchangeRate")]
        public async Task<bool> DeletedFixExchangeRate([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int currency, [FromQuery] string accoutantNumber)
        {
            return await _calcBillService.DeletedFixExchangeRate(fromDate, toDate, currency, accoutantNumber);
        }

        [HttpGet]
        [Route("CheckExistedCostTransfer")]
        public async Task<bool> CheckExistedCostTransfer([FromQuery] EnumCostTransfer type, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _calcBillService.CheckExistedCostTransfer(type, fromDate, toDate);
        }

        [HttpDelete]
        [Route("DeletedCostTransfer")]
        public async Task<bool> DeletedCostTransfer([FromQuery] EnumCostTransfer type, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _calcBillService.DeletedCostTransfer(type, fromDate, toDate);
        }

        [HttpGet]
        [Route("CalcCostTransferBalanceZero")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcCostTransferBalanceZero([FromQuery] long toDate)
        {
            return await _calcBillService.CalcCostTransferBalanceZero(toDate);
        }

        [HttpGet]
        [Route("CheckExistedCostTransferBalanceZero")]
        public async Task<bool> CheckExistedCostTransferBalanceZero([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _calcBillService.CheckExistedCostTransferBalanceZero(fromDate, toDate);
        }

        [HttpDelete]
        [Route("DeletedCostTransferBalanceZero")]
        public async Task<bool> DeletedCostTransferBalanceZero([FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _calcBillService.DeletedCostTransferBalanceZero(fromDate, toDate);
        }

        [HttpGet]
        [Route("CalcDepreciation")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcDepreciation([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] string accountNumber)
        {
            return await _calcBillService.CalcDepreciation(fromDate, toDate, accountNumber);
        }

        [HttpGet]
        [Route("CheckExistedDepreciation")]
        public async Task<bool> CheckExistedDepreciation([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] string accountNumber)
        {
            return await _calcBillService.CheckExistedDepreciation(fromDate, toDate, accountNumber);
        }

        [HttpDelete]
        [Route("DeletedDepreciation")]
        public async Task<bool> DeletedDepreciation([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] string accountNumber)
        {
            return await _calcBillService.DeletedDepreciation(fromDate, toDate, accountNumber);
        }

        [HttpGet]
        [Route("CalcPrepaidExpense")]
        public async Task<ICollection<NonCamelCaseDictionary>> CalcPrepaidExpense([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] string accountNumber)
        {
            return await _calcBillService.CalcPrepaidExpense(fromDate, toDate, accountNumber);
        }

        [HttpGet]
        [Route("CheckExistedPrepaidExpense")]
        public async Task<bool> CheckExistedPrepaidExpense([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] string accountNumber)
        {
            return await _calcBillService.CheckExistedPrepaidExpense(fromDate, toDate, accountNumber);
        }

        [HttpDelete]
        [Route("DeletedPrepaidExpense")]
        public async Task<bool> DeletedPrepaidExpense([FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] string accountNumber)
        {
            return await _calcBillService.DeletedPrepaidExpense(fromDate, toDate, accountNumber);
        }

    }
}
