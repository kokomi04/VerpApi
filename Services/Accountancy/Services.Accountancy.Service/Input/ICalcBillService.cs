using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface ICalcBillService
    {
        Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate(long toDate, int currency, int exchangeRate);

        Task<bool> CheckExistedFixExchangeRate(long fromDate, long toDate);

        Task<bool> DeletedFixExchangeRate(long fromDate, long toDate);

        Task<ICollection<NonCamelCaseDictionary>> CalcCostTransfer(long toDate, EnumCostTransfer type, bool byDepartment, bool byCustomer, bool byFixedAsset, bool byExpenseItem, bool byFactory, bool byProduct, bool byStock);

        ICollection<CostTransferTypeModel> GetCostTransferTypes();

        Task<bool> CheckExistedCostTransfer(EnumCostTransfer type, long fromDate, long toDate);

        Task<bool> DeletedCostTransfer(EnumCostTransfer type, long fromDate, long toDate);

        Task<ICollection<NonCamelCaseDictionary>> CalcCostTransferBalanceZero(long toDate);

        Task<bool> CheckExistedCostTransferBalanceZero(long fromDate, long toDate);

        Task<bool> DeletedCostTransferBalanceZero(long fromDate, long toDate);

        Task<ICollection<NonCamelCaseDictionary>> CalcDepreciation(long fromDate, long toDate, string accountNumber);

        Task<bool> CheckExistedDepreciation(long fromDate, long toDate, string accountNumber);

        Task<bool> DeletedDepreciation(long fromDate, long toDate, string accountNumber);

    }
}
