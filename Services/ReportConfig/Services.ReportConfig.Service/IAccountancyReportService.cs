using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using VErp.Infrastructure.ServiceCore.Model;

namespace Verp.Services.ReportConfig.Service
{
    public interface IAccountancyReportService
    {
        Task<ReportDataModel> Report(int reportId, ReportFilterModel model);
    }
}
