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
using VErp.Services.Accountancy.Model.Input;

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
            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@TyGia", exchangeRate),
                new SqlParameter("@Currency", currency)
            };
            var data = await _accountancyDBContext.ExecuteDataProcedure("usp_TK_CalcFixExchangeRate", sqlParams);
            var rows = data.ConvertData();
            return rows;
        }

        public async Task<ICollection<NonCamelCaseDictionary>> CalcCostTransfer(long toDate, EnumCostTransfer type, bool byDepartment, bool byCustomer, bool byFixedAsset,
            bool byExpenseItem, bool byFactory, bool byProduct, bool byStock)
        {
            SqlParameter[] sqlParams = new SqlParameter[]
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
