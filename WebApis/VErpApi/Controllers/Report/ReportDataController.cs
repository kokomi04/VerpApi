using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.Report
{
    [Route("api/reports/accoutancy")]
    [ObjectDataApi(EnumObjectType.ReportType, "reportId")]
    public class ReportDataController : VErpBaseController
    {
        private readonly IDataReportService _accountancyReportService;
        public ReportDataController(IDataReportService accountancyReportService)
        {
            _accountancyReportService = accountancyReportService;
        }



        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("view/{reportId}")]
        public async Task<ReportDataModel> ReportView([FromRoute] int reportId, [FromBody] ReportFilterModel model)
        {
            return await _accountancyReportService.Report(reportId, model, model?.Page ?? 0, model?.Size ?? 0)
                .ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("view/{reportId}/asDocument")]
        public async Task<IActionResult> ASDocument([FromRoute] int reportId, [FromBody] ReportDataModel dataModel)
        {
            var r = await _accountancyReportService.GenerateReportAsPdf(reportId, dataModel);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("view/{reportId}/asExcel")]
        public async Task<FileStreamResult> AsExcel([FromRoute] int reportId, [FromBody] ReportFacadeModel model)
        {
            var (stream, fileName, contentType) = await _accountancyReportService.ExportExcel(reportId, model);

            return new FileStreamResult(stream, contentType) { FileDownloadName = fileName };
        }
    }


}