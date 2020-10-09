﻿using System;
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

        [HttpPost]
        [Route("view/{reportId}/asDocument")]
        public async Task<IActionResult> ASDocument([FromRoute] int reportId, [FromBody] ReportDataModel dataModel)
        {
            var r = await _accountancyReportService.GenerateReportAsPdf(reportId, dataModel);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }

        [HttpPost]
        [Route("view/{reportId}/asExcel")]
        public async Task<FileStreamResult> AsExcel([FromRoute] int reportId, [FromBody] ReportFacadeModel model)
        {
            var (stream, fileName, contentType) = await _accountancyReportService.ExportExcel(reportId, model);

            return new FileStreamResult(stream, contentType) { FileDownloadName = fileName };
        }
    }

   
}