﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class CalcBillPrivateService : CalcBillServiceBase, ICalcBillPrivateService
    {
        public CalcBillPrivateService(AccountancyDBPrivateContext accountancyDBContext, ICurrentContextService currentContextService, IInputDataPrivateService inputDataPrivateService) : base(accountancyDBContext, currentContextService, inputDataPrivateService)
        {

        }
    }

    public class CalcBillPublicService : CalcBillServiceBase, ICalcBillPublicService
    {
        public CalcBillPublicService(AccountancyDBPublicContext accountancyDBContext, ICurrentContextService currentContextService, IInputDataPublicService inputDataPublicService) : base(accountancyDBContext, currentContextService, inputDataPublicService)
        {

        }
    }

    public abstract class CalcBillServiceBase : ICalcBillServiceBase
    {
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IInputDataServiceBase _inputDataService;

        public CalcBillServiceBase(AccountancyDBContext accountancyDBContext, ICurrentContextService currentContextService, IInputDataServiceBase inputDataService)
        {
            _accountancyDBContext = accountancyDBContext;
            _currentContextService = currentContextService;
            _inputDataService = inputDataService;
        }


        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate(CalcFixExchangeRateRequestModel req)
        {
            if (req == null) throw GeneralCode.InvalidParams.BadRequest();

            if (req.AccountNumber == null) req.AccountNumber = string.Empty;
            if (req.PartnerIds == null) req.PartnerIds = new List<string>();

            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@ToDate", req.ToDate.UnixToDateTime()),
                new SqlParameter("@TyGia", req.ExchangeRate),
                new SqlParameter("@CurrencyId", req.CurrencyId),
                new SqlParameter("@AccountNumber", req.AccountNumber),
                req.PartnerIds.ToSqlParameter("@PartnerIds")
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcFixExchangeRate", sqlParams);
            var rows = data.ConvertData();
            return rows;
        }

        public async Task<DataResultModel> FixExchangeRateDetail(long fromDate, long toDate, int currency, string accountNumber, string partnerId)
        {
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@Kh", partnerId),
                new SqlParameter("@Du_no_dau_ky_vnd", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_co_dau_ky_vnd", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_no_dau_ky_ngoai_te", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_co_dau_ky_ngoai_te", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_no_cuoi_ky_vnd", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_co_cuoi_ky_vnd", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_no_cuoi_ky_ngoai_te", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_co_cuoi_ky_ngoai_te", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 }
            };

            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_FixExchangeRateDetail", sqlParams);
            var rows = data.ConvertData();
            var head = new NonCamelCaseDictionary();
            foreach (var param in sqlParams)
            {
                if (param.Direction != ParameterDirection.Output) continue;
                head.Add(param.ParameterName.TrimStart('@'), (param.Value as decimal?).GetValueOrDefault());
            }
            var result = new DataResultModel
            {
                Rows = rows,
                Head = head
            };
            return result;
        }

        public async Task<bool> CheckExistedFixExchangeRate(long fromDate, long toDate, int currency, string accoutantNumber)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            if (accoutantNumber == null) accoutantNumber = string.Empty;
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@AccoutantNumber", accoutantNumber),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_CheckExistedFixExchangeRate", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<bool> DeletedFixExchangeRate(long fromDate, long toDate, int currency, string accoutantNumber)
        {
            if (accoutantNumber == null) accoutantNumber = string.Empty;

            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);

            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fDate),
                new SqlParameter("@ToDate", tDate),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@AccoutantNumber", accoutantNumber),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_DeleteFixExchangeRate", sqlParams, true);
            return (result.Value as bool?).GetValueOrDefault();
        }


        public async Task<ICollection<NonCamelCaseDictionary>> CalcCostTransfer(long toDate, EnumCostTransfer type, bool byDepartment, bool byCustomer, bool byFixedAsset,
            bool byExpenseItem, bool byFactory, bool byProduct, bool byStock)
        {
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Type", (int)type),
                new SqlParameter("@by_bo_phan", byDepartment),
                new SqlParameter("@by_kh", byCustomer),
                new SqlParameter("@by_tscd", byFixedAsset),
                new SqlParameter("@by_khoan_muc_cp", byExpenseItem),
                new SqlParameter("@by_phan_xuong", byFactory),
                new SqlParameter("@by_vthhtp", byProduct),
                new SqlParameter("@by_kho", byStock)
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcCostTransfer", sqlParams);
            var rows = data.ConvertData();
            return rows;
        }

        public async Task<DataResultModel> CalcCostTransferDetail(string tk, long fromDate, long toDate, EnumCostTransfer type,
            bool byDepartment, bool byCustomer, bool byFixedAsset, bool byExpenseItem, bool byFactory, bool byProduct, bool byStock,
            int? department, string customer, int? fixedAsset, int? expenseItem, int? factory, int? product, int? stock)
        {
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Tk", tk),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Type", (int)type),
                new SqlParameter("@by_bo_phan", byDepartment),
                new SqlParameter("@by_kh", byCustomer),
                new SqlParameter("@by_tscd", byFixedAsset),
                new SqlParameter("@by_khoan_muc_cp", byExpenseItem),
                new SqlParameter("@by_phan_xuong", byFactory),
                new SqlParameter("@by_vthhtp", byProduct),
                new SqlParameter("@by_kho", byStock),
                byDepartment && department.HasValue ? new SqlParameter("@bo_phan", department.Value) : new SqlParameter("@bo_phan", DBNull.Value),
                byCustomer && !string.IsNullOrEmpty(customer) ? new SqlParameter("@kh", customer) : new SqlParameter("@kh", DBNull.Value),
                byFixedAsset && fixedAsset.HasValue ? new SqlParameter("@tscd", fixedAsset.Value) : new SqlParameter("@tscd", DBNull.Value),
                byExpenseItem && expenseItem.HasValue ? new SqlParameter("@khoan_muc_cp", expenseItem.Value) : new SqlParameter("@khoan_muc_cp", DBNull.Value),
                byFactory && factory.HasValue ? new SqlParameter("@phan_xuong", factory.Value) : new SqlParameter("@phan_xuong", DBNull.Value),
                byProduct && product.HasValue ? new SqlParameter("@vthhtp", product.Value) : new SqlParameter("@vthhtp", DBNull.Value),
                byStock && stock.HasValue ? new SqlParameter("@kho", stock.Value) : new SqlParameter("@kho", DBNull.Value),
                new SqlParameter("@Du_no_dau_ky", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 },
                new SqlParameter("@Du_no_cuoi_ky", 0) { SqlDbType = SqlDbType.Decimal, Direction = ParameterDirection.Output, Precision = 24, Scale = 5 }
            };

            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcCostTransferDetail", sqlParams);
            var rows = data.ConvertData();
            var head = new NonCamelCaseDictionary();
            foreach (var param in sqlParams)
            {
                if (param.Direction != ParameterDirection.Output) continue;
                head.Add(param.ParameterName.TrimStart('@'), (param.Value as decimal?).GetValueOrDefault());
            }
            var result = new DataResultModel
            {
                Rows = rows,
                Head = head
            };
            return result;
        }


        #region CalcFixExchangeRateByOrder
        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRateByOrder(long fromDate, long toDate, int currency, string tk)
        {
            if (tk == null) tk = string.Empty;
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@Tk", tk),
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcFixExchangeRateByOrder", sqlParams);
            var rows = data.ConvertData();
            return rows;
        }


        public async Task<bool> CheckExistedFixExchangeRateByOrder(long fromDate, long toDate, int currency, string tk)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            if (tk == null) tk = string.Empty;
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@Tk", tk),
                result
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CheckExistedCalcFixExchangeRateByOrder", sqlParams);
            return (result.Value as bool?).GetValueOrDefault();
        }



        public async Task<bool> DeletedFixExchangeRateByOrder(long fromDate, long toDate, int currency, string tk)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);

            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            if (tk == null) tk = string.Empty;
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fDate),
                new SqlParameter("@ToDate", tDate),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@Tk", tk),
                result
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_DeleteCalcFixExchangeRateByOrder", sqlParams);
            return (result.Value as bool?).GetValueOrDefault();
        }
        #endregion

        #region CalcFixExchangeRateByLoanConvenant
        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRateByLoanCovenant(long fromDate, long toDate, int currency, string tk)
        {
            if (tk == null) tk = string.Empty;
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@Tk", tk),
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcFixExchangeRateByLoanConvenant", sqlParams);
            var rows = data.ConvertData();
            return rows;
        }

        public async Task<bool> CheckExistedFixExchangeRateByLoanCovenant(long fromDate, long toDate, int currency, string tk)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            if (tk == null) tk = string.Empty;
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@Tk", tk),
                result
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CheckExistedCalcFixExchangeRateByLoanConvenant", sqlParams);
            return (result.Value as bool?).GetValueOrDefault();
        }



        public async Task<bool> DeleteFixExchangeRateByLoanCovenant(long fromDate, long toDate, int currency, string tk)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);

            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            if (tk == null) tk = string.Empty;
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate",fDate),
                new SqlParameter("@ToDate", tDate),
                new SqlParameter("@Currency", currency),
                new SqlParameter("@Tk", tk),
                result
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_DeleteCalcFixExchangeRateByLoanConvenant", sqlParams);
            return (result.Value as bool?).GetValueOrDefault();
        }
        #endregion

        public ICollection<CostTransferTypeModel> GetCostTransferTypes()
        {
            var types = EnumExtensions.GetEnumMembers<EnumCostTransfer>().Select(m => new CostTransferTypeModel
            {
                Title = m.Description,
                Value = (int)m.Enum
            }).ToList();
            return types;
        }

        public async Task<bool> CheckExistedCostTransfer(EnumCostTransfer type, long fromDate, long toDate)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@Type", (int)type),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_CheckExistedCostTransfer", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<bool> DeletedCostTransfer(EnumCostTransfer type, long fromDate, long toDate)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);


            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fDate),
                new SqlParameter("@ToDate", tDate),
                new SqlParameter("@Type", (int)type),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_DeleteCostTransfer", sqlParams, true);
            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<ICollection<NonCamelCaseDictionary>> CalcCostTransferBalanceZero(long toDate)
        {
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@ToDate", toDate.UnixToDateTime())
            };

            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcCostTransferBalanceZero", sqlParams);
            var rows = data.ConvertData();
            return rows;
        }

        public async Task<bool> CheckExistedCostTransferBalanceZero(long fromDate, long toDate)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_CheckExistedCostTransferBalanceZero", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<bool> DeletedCostTransferBalanceZero(long fromDate, long toDate)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);

            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fDate),
                new SqlParameter("@ToDate", tDate),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_DeleteCostTransferBalanceZero", sqlParams, true);
            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<ICollection<NonCamelCaseDictionary>> CalcDepreciation(long fromDate, long toDate, string accountNumber)
        {
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@TimeZoneOffset", _currentContextService.TimeZoneOffset)
            };

            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcDepreciationV2", sqlParams);
            var rows = data.ConvertData();
            return rows;
        }

        public async Task<bool> CheckExistedDepreciation(long fromDate, long toDate, string accountNumber)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_CheckExistedDepreciation", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<bool> DeletedDepreciation(long fromDate, long toDate, string accountNumber)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);

            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@FromDate", fDate),
                new SqlParameter("@ToDate", tDate),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_DeleteDepreciation", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }


        public async Task<ICollection<NonCamelCaseDictionary>> CalcPrepaidExpense(long fromDate, long toDate, string accountNumber)
        {
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@TimeZoneOffset", _currentContextService.TimeZoneOffset)
            };

            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcPrepaidExpenseV2", sqlParams, AccountantConstants.REPORT_QUERY_TIMEOUT);
            var rows = data.ConvertData();
            return rows;
        }

        public async Task<bool> CheckExistedPrepaidExpense(long fromDate, long toDate, string accountNumber)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_CheckExistedPrepaidExpense", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<bool> DeletedPrepaidExpense(long fromDate, long toDate, string accountNumber)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();

            await _inputDataService.ValidateAccountantConfig(fDate, null);


            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@FromDate", fDate),
                new SqlParameter("@ToDate", tDate),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_DeletePrepaidExpense", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }
    }
}
