using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.Report
{
    [Route("api/reports/accoutancy")]
    public class ReportDataController : VErpBaseController
    {
        private readonly IAccountancyReportService _accountancyReportService;
        public ReportDataController(IAccountancyReportService accountancyReportService)
        {
            _accountancyReportService = accountancyReportService;
        }



        [HttpPost]
        [Route("view/{reportId}")]
        public async Task<ReportDataModel> ReportView([FromRoute] int reportId, [FromBody] ReportFilterModel model)
        {
            return await _accountancyReportService.Report(reportId, model)
                .ConfigureAwait(true);
        }
    }

   
}