using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using Verp.Resources.Master.Config.DataConfig;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.EF.EFExtensions;
using DocumentFormat.OpenXml.Spreadsheet;
using OpenXmlPowerTools;

namespace VErp.Infrastructure.ServiceCore.Abstract
{
    public class BillDateValidateionServiceAbstract
    {
        private static readonly DateTime MINIMUM_OF_DATE = new DateTime(2010, 1, 1);
        private const int DEFAULT_TIMEZONE_OFFSET = -420;
        private readonly HashSet<ValidateBillDate> _validateCaches = new HashSet<ValidateBillDate>();

        private readonly DbContext _dbContext;
        protected BillDateValidateionServiceAbstract(DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected async Task ValidateDateOfBill(DateTime? billDate, DateTime? oldDate)
        {
            var validated = new ValidateBillDate() { BillDate = billDate, OldDate = oldDate };
            if (_validateCaches.Contains(validated)) return;

            await ValidateDateOfBillQuery(billDate, oldDate);

            _validateCaches.Add(validated);
        }



        private async Task ValidateDateOfBillQuery(DateTime? billDate, DateTime? oldDate)
        {
            var timezoneOffset = DEFAULT_TIMEZONE_OFFSET;
            if (_dbContext is ISubsidiayRequestDbContext requestDbContext)
            {
                timezoneOffset = requestDbContext.CurrentContextService.TimeZoneOffset ?? DEFAULT_TIMEZONE_OFFSET;

            }

            if (billDate != null || oldDate != null)
            {

                var result = new SqlParameter("@ResStatus", false) { Direction = ParameterDirection.Output };
                var sqlParams = new List<SqlParameter>
                {
                    result
                };

                if (oldDate.HasValue)
                {
                    if (oldDate.Value < MINIMUM_OF_DATE)
                    {
                        throw BillDateValidateionMessage.BillDateLessThanMinimum.BadRequestFormat(oldDate.Value.AddMinutes(-timezoneOffset));
                    }
                    sqlParams.Add(new SqlParameter("@OldDate", SqlDbType.DateTime2) { Value = oldDate });
                }

                if (billDate.HasValue)
                {
                    if (billDate.Value < MINIMUM_OF_DATE)
                    {
                        throw BillDateValidateionMessage.BillDateLessThanMinimum.BadRequestFormat(billDate.Value.AddMinutes(-timezoneOffset));
                    }
                    sqlParams.Add(new SqlParameter("@BillDate", SqlDbType.DateTime2) { Value = billDate });
                }

                sqlParams.Add(new SqlParameter("@TimeZoneOffset", SqlDbType.Int) { Value = timezoneOffset });

                await _dbContext.ExecuteStoreProcedure("asp_ValidateBillDate", sqlParams, true);

                if (!(result.Value as bool?).GetValueOrDefault())
                    throw BillDateValidateionMessage.BillDateLocked.BadRequest();
            }
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
