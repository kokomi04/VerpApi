using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Model.Dashboard;
using VErp.Services.Accountancy.Service.InputDashboard;

namespace VErpApi.Controllers.Accountancy.Dashboard
{
    [Route("api/accountancy/data/bills")]
    public class InputDashboardController: VErpBaseController
    {
        private readonly IInputDashboardService _inputDashboardService;

        public InputDashboardController(IInputDashboardService inputDashboardService)
        {
            _inputDashboardService = inputDashboardService;
        }

        [HttpGet]
        [Route("revenueAndProfit")]
        public async Task<IList<RevenueAndProfirByMonthModel>> GetRevenueAndProfitByMonth([FromQuery]long fromDate, [FromQuery] long toDate)
        {
            return await _inputDashboardService.GetRevenueAndProfitByMonth(fromDate, toDate);
        }

    }
}