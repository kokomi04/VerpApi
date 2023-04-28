using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;

namespace VErpApi.Controllers.Report
{
    [Route("api/Reports")]
    [ObjectDataApi(EnumObjectType.ReportType, "reportId")]
    public class ReportsController : VErpBaseController
    {
        private readonly IDataReportService _dataReportService;
        private readonly IFilterSettingReportService _filterSettingReportService;
        public ReportsController(IDataReportService accountancyReportService, IFilterSettingReportService filterSettingReportService)
        {
            _dataReportService = accountancyReportService;
            _filterSettingReportService = filterSettingReportService;
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("view/{reportId}")]
        public async Task<ReportDataModel> ReportView([FromRoute] int reportId, [FromBody] ReportFilterModel model)
        {
            return await _dataReportService.Report(reportId, model, model?.Page ?? 0, model?.Size ?? 0)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("setting/{reportId}")]
        public async Task<Dictionary<int, object>> Setting([FromRoute] int reportId)
        {
            return await _filterSettingReportService.Get(reportId);
        }

        [HttpPut]
        [Route("setting/{reportId}")]
        public async Task<bool> Setting([FromRoute] int reportId, [FromBody] Dictionary<int, object> fieldValues)
        {
            return await _filterSettingReportService.Update(reportId, fieldValues);
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("view/{reportId}/asDocument")]
        public async Task<IActionResult> ASDocument([FromRoute] int reportId, [FromBody] ReportDataModel dataModel)
        {
            var r = await _dataReportService.GenerateReportAsPdf(reportId, dataModel);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("view/{reportId}/asExcel")]
        public async Task<FileStreamResult> AsExcel([FromRoute] int reportId, [FromBody] ReportFacadeModel model)
        {
            var (stream, fileName, contentType) = await _dataReportService.ExportExcel(reportId, model);

            return new FileStreamResult(stream, contentType) { FileDownloadName = fileName };
        }
    }


}