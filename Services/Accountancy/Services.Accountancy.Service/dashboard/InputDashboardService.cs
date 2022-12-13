using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Services.Accountancy.Model.Dashboard;

namespace VErp.Services.Accountancy.Service.InputDashboard
{
    public interface IInputDashboardService
    {
        Task<IList<RevenueAndProfirByMonthModel>> GetRevenueAndProfitByMonth(long fromDate, long toDate);
    }

    public class InputDashboardService : IInputDashboardService
    {
        private readonly AccountancyDBPrivateContext _accountancyDBContext;

        public InputDashboardService(AccountancyDBPrivateContext accountancyDBContext)
        {
            _accountancyDBContext = accountancyDBContext;
        }

        public async Task<IList<RevenueAndProfirByMonthModel>> GetRevenueAndProfitByMonth(long fromDate, long toDate)
        {
            var querySQL = @$"
                SELECT
                    YEAR(v.ngay_ct) [Year], MONTH(v.ngay_ct) [Month],
                    SUM(v.Revenue) Revenue,
                    SUM(v.Profit) Profit
                FROM (
                    SELECT
                        ISNULL(r.ngay_ct, p.ngay_ct) ngay_ct,
                        r.Revenue,
                        p.Profit
                    FROM (SELECT
                        v.ngay_ct,
                        v.vnd0 Revenue
                        FROM InputValueRow v
                        WHERE (v.tk_co0 LIKE '511%'
                        OR v.tk_co0 LIKE '5112%')
                        AND (v.tk_no0 LIKE '131%'
                        OR v.tk_no0 LIKE '111%'
                        OR v.tk_no0 LIKE '112%'
                        OR v.tk_no0 LIKE '136%'
                        OR v.tk_no0 LIKE '138%'
                        OR v.tk_no0 LIKE '338%'
                        OR v.tk_no0 LIKE '336%')
                        AND v.IsDeleted = 0
                        AND v.vnd0 > 0) r
                    FULL OUTER JOIN (SELECT
                        v.ngay_ct,
                        v.vnd0 Profit
                        FROM InputValueRow v
                        WHERE v.tk_co0 LIKE '421%'
                        OR v.tk_no0 LIKE '421%'
                        AND v.IsDeleted = 0
                        AND v.vnd0 > 0) p
                    ON r.ngay_ct = p.ngay_ct) v
                WHERE v.ngay_ct >= @FromDate AND v.ngay_ct <= @ToDate
                GROUP BY YEAR(v.ngay_ct), MONTH(v.ngay_ct)
            ";

            var resultData = (await _accountancyDBContext.QueryDataTable(querySQL.ToString(), new SqlParameter[]
                                {
                                    new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                                    new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                                }))
                            .ConvertData<RevenueAndProfirByMonthModel>()
                            .OrderBy(x => x.Year).ThenBy(x => x.Month)
                            .ToList();
            return resultData;
        }

    }
}