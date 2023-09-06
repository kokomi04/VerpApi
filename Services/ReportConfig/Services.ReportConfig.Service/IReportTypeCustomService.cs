using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;

namespace Verp.Services.ReportConfig.Service
{
    public interface IReportTypeCustomService
    {
        Task<int> AddReportTypeCustom(ReportTypeCustomImportModel data);
        Task<int> UpdateReportTypeCustom(int reportTypeId, ReportTypeCustomImportModel data);
        Task<bool> DeleteReportTypeCustom(int reportTypeId);
        Task<ReportTypeCustomModel> InfoReportTypeCustom(int reportTypeId);
    }
}
