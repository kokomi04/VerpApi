using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Report;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Service.Report
{
    public interface IReportService
    {
        Task<IList<StepModel>> GetSteps(long startDate, long endDate);
        Task<IList<StepProgressModel>> GetProductionProgressReport(long startDate, long endDate, int[] stepIds);
    }
}
