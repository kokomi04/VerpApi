using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using static Verp.Resources.Stock.Inventory.Abstract.InventoryAbstractMessage;


namespace VErp.Services.Stock.Service.Inventory.Implement.Abstract
{
    public abstract class InventoryBillDateAbstract
    {
        protected readonly StockDBContext _stockDbContext;
        internal InventoryBillDateAbstract(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
        }

        private readonly HashSet<ValidateBillDate> _validateCaches = new HashSet<ValidateBillDate>();
        protected async Task ValidateBill(DateTime? billDate, DateTime? oldDate)
        {
            var validated = new ValidateBillDate() { BillDate = billDate, OldDate = oldDate };
            if (_validateCaches.Contains(validated)) return;

            if (billDate != null || oldDate != null)
            {

                var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
                var sqlParams = new List<SqlParameter>
                {
                    result
                };

                if (oldDate.HasValue)
                {
                    sqlParams.Add(new SqlParameter("@OldDate", SqlDbType.DateTime2) { Value = oldDate });
                }

                if (billDate.HasValue)
                {
                    sqlParams.Add(new SqlParameter("@BillDate", SqlDbType.DateTime2) { Value = billDate });
                }

                await _stockDbContext.ExecuteStoreProcedure("asp_ValidateBillDate", sqlParams, true);

                if (!(result.Value as bool?).GetValueOrDefault())
                    throw BillDateLocked.BadRequest();
            }

            _validateCaches.Add(validated);
        }

        private struct ValidateBillDate
        {
            public DateTime? BillDate;
            public DateTime? OldDate;
            public static bool operator ==(ValidateBillDate c1, ValidateBillDate c2)
            {
                return c1.BillDate == c2.BillDate && c1.OldDate == c2.OldDate;
            }

            public static bool operator !=(ValidateBillDate c1, ValidateBillDate c2)
            {
                return c1.BillDate != c2.BillDate || c1.OldDate != c2.OldDate;
            }

            public override bool Equals(object obj)
            {
                return this == (ValidateBillDate)obj;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
