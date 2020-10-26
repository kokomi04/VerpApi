using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using System;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class CalcBillService : ICalcBillService
    {
        private readonly AccountancyDBContext _accountancyDBContext;

        public CalcBillService(AccountancyDBContext accountancyDBContext)
        {
            _accountancyDBContext = accountancyDBContext;
        }


        public async Task<ICollection<NonCamelCaseDictionary>> CalcFixExchangeRate(long toDate, int currency, int exchangeRate)
        {
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@TyGia", exchangeRate),
                new SqlParameter("@Currency", currency)
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

        public async Task<DataResultModel> CalcCostTransferDetail(long fromDate, long toDate, EnumCostTransfer type,
            bool byDepartment, bool byCustomer, bool byFixedAsset, bool byExpenseItem, bool byFactory, bool byProduct, bool byStock,
            int? department, string customer, int? fixedAsset, int? expenseItem, int? factory, int? product, int? stock)
        {
            var sqlParams = new SqlParameter[]
            {
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

        public async Task<bool> CheckExistedFixExchangeRate(long fromDate, long toDate)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_CheckExistedFixExchangeRate", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }

        public async Task<bool> DeletedFixExchangeRate(long fromDate, long toDate)
        {
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_DeleteFixExchangeRate", sqlParams, true);
            return (result.Value as bool?).GetValueOrDefault();
        }

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
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
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
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
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
                new SqlParameter("@ToDate", toDate.UnixToDateTime())
            };

            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcDepreciation", sqlParams);
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
            var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@SoTK", accountNumber),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                result
            };
            await _accountancyDBContext.ExecuteStoreProcedure("usp_TK_DeleteDepreciation", sqlParams, true);

            return (result.Value as bool?).GetValueOrDefault();
        }

    }
}
