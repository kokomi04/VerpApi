using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service.Implement;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;

namespace VErpApi.Controllers.Report
{
    [Route("api/DashboardData")]
    [ObjectDataApi(EnumObjectType.DashboardType, "dashboardTypeId")]
    public class DashboardDataController : VErpBaseController
    {
        private readonly IDataDashboardService _dataDashboardService;

        public DashboardDataController(IDataDashboardService dataDashboardService)
        {
            _dataDashboardService = dataDashboardService;
        }

        [HttpPost]
        [Route("view/{dashboardTypeId}")]
        public async Task<IList<NonCamelCaseDictionary>> Dashboard([FromRoute] int dashboardTypeId, [FromBody] ReportFilterDataModel model)
        {
            return await _dataDashboardService.Dashboard(dashboardTypeId, model);
        }
    }
}