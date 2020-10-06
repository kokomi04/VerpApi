using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace Verp.Services.ReportConfig.Service
{
    public interface IAccountancyReportService
    {
        Task<ReportDataModel> Report(int reportId, ReportFilterModel model);
        Task<(Stream file, string contentType, string fileName)> GenerateReportAsPdf(int reportId, ReportDataModel dataModel);
        Task<(MemoryStream Stream, string FileName)> ExportExcel(int reportId, ReportFilterModel model);
    }
}
