using System.IO;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;

namespace Verp.Services.ReportConfig.Service
{
    public interface IDataReportService
    {
        Task<ReportDataModel> Report(int reportId, ReportFilterDataModel model, int page, int size);
        Task<(Stream file, string contentType, string fileName)> GenerateReportAsPdf(int reportId, ReportDataModel dataModel);
        Task<(Stream stream, string fileName, string contentType)> ExportExcel(int reportId, ReportFacadeModel model);
    }
}
