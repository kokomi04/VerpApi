﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface ICalcBillServiceBase
    {
        Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate(CalcFixExchangeRateRequestModel req);

        Task<DataResultModel> FixExchangeRateDetail(long fromDate, long toDate, int currency, string accountNumber, string partnerId);

        Task<bool> CheckExistedFixExchangeRate(long fromDate, long toDate, int currency, string accoutantNumber);

        Task<bool> DeletedFixExchangeRate(long fromDate, long toDate, int currency, string accoutantNumber);

        Task<ICollection<NonCamelCaseDictionary>> CalcCostTransfer(long toDate, EnumCostTransfer type,
            bool byDepartment, bool byCustomer, bool byFixedAsset, bool byExpenseItem, bool byFactory, bool byProduct, bool byStock);

        Task<DataResultModel> CalcCostTransferDetail(string tk, long fromDate, long toDate, EnumCostTransfer type,
            bool byDepartment, bool byCustomer, bool byFixedAsset, bool byExpenseItem, bool byFactory, bool byProduct, bool byStock,
            int? department, string customer, int? fixedAsset, int? expenseItem, int? factory, int? product, int? stock);

        Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRateByOrder(long fromDate, long toDate, int currency, string tk);

        Task<bool> CheckExistedFixExchangeRateByOrder(long fromDate, long toDate, int currency, string tk);

        Task<bool> DeletedFixExchangeRateByOrder(long fromDate, long toDate, int currency, string tk);

        Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRateByLoanCovenant(long fromDate, long toDate, int currency, string tk);

        Task<bool> CheckExistedFixExchangeRateByLoanCovenant(long fromDate, long toDate, int currency, string tk);

        Task<bool> DeleteFixExchangeRateByLoanCovenant(long fromDate, long toDate, int currency, string tk);

        ICollection<CostTransferTypeModel> GetCostTransferTypes();

        Task<bool> CheckExistedCostTransfer(EnumCostTransfer type, long fromDate, long toDate);

        Task<bool> DeletedCostTransfer(EnumCostTransfer type, long fromDate, long toDate);

        Task<ICollection<NonCamelCaseDictionary>> CalcCostTransferBalanceZero(long toDate);

        Task<bool> CheckExistedCostTransferBalanceZero(long fromDate, long toDate);

        Task<bool> DeletedCostTransferBalanceZero(long fromDate, long toDate);

        Task<ICollection<NonCamelCaseDictionary>> CalcDepreciation(long fromDate, long toDate, string accountNumber);

        Task<bool> CheckExistedDepreciation(long fromDate, long toDate, string accountNumber);

        Task<bool> DeletedDepreciation(long fromDate, long toDate, string accountNumber);

        Task<ICollection<NonCamelCaseDictionary>> CalcPrepaidExpense(long fromDate, long toDate, string accountNumber);

        Task<bool> CheckExistedPrepaidExpense(long fromDate, long toDate, string accountNumber);

        Task<bool> DeletedPrepaidExpense(long fromDate, long toDate, string accountNumber);
    }

    public interface ICalcBillPrivateService : ICalcBillServiceBase
    {

    }

    public interface ICalcBillPublicService : ICalcBillServiceBase
    {

    }
}
