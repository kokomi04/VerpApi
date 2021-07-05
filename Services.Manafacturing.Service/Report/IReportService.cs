using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Report;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Service.Report
{
    public interface IReportService
    {
        Task<IList<StepModel>> GetSteps(long fromDate, long toDate);
        Task<IList<StepProgressModel>> GetProductionProgressReport(long fromDate, long toDate, int[] stepIds);
        Task<ProductionOrderStepModel> GetProductionOrderStepProgress(long fromDate, long toDate);
        Task<IList<ProductionReportModel>> GetProductionOrderReport(long fromDate, long toDate);
        Task<IList<ProcessingOrderListModel>> GetProcessingOrderList();
        Task<IList<StepReportModel>> GetProcessingStepReport(long productionOrderId, int[] stepIds);
        Task<IList<OutsourcePartRequestReportModel>> GetOursourcePartRequestReport(long fromDate, long toDate, long? productionOrderId);
        Task<PageData<OutsourceStepRequestReportModel>> GetOursourceStepRequestReport(int page, int size, string orderByFieldName, bool asc, Clause filters);
    }
}
