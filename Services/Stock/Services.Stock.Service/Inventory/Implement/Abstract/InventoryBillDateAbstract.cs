using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.StockDB;
using static VErp.Services.Stock.Service.Resources.Inventory.Abstract.InventoryAbstractMessage;


namespace VErp.Services.Stock.Service.Inventory.Implement.Abstract
{
    public abstract class InventoryBillDateAbstract
    {
        protected readonly StockDBContext _stockDbContext;
        public InventoryBillDateAbstract(StockDBContext stockDbContext)
        {
            _stockDbContext = stockDbContext;
        }
        protected async Task ValidateBill(DateTime? billDate, DateTime? oldDate)
        {
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
        }
    }
}
