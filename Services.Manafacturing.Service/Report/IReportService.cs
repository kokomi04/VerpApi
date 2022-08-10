using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Report;
using VErp.Services.Manafacturing.Model.Step;

namespace VErp.Services.Manafacturing.Service.Report
{
    public interface IReportService
    {
        Task<IList<StepModel>> GetSteps(int? monthPlanId, long fromDate, long toDate);
        Task<IList<StepProgressModel>> GetProductionProgressReport(int? monthPlanId, long fromDate, long toDate, int[] stepIds);
        Task<ProductionOrderStepModel> GetProductionOrderStepProgress(int? monthPlanId, long fromDate, long toDate);
        Task<IList<ProductionReportModel>> GetProductionOrderReport(int? monthPlanId, long fromDate, long toDate);
        Task<IList<ProcessingOrderListModel>> GetProcessingOrderList();
        Task<IList<StepReportModel>> GetProcessingStepReport(long productionOrderId, int[] stepIds);
        Task<IList<OutsourcePartRequestReportModel>> GetOursourcePartRequestReport(long fromDate, long toDate, long? productionOrderId);
        Task<PageData<OutsourceStepRequestReportModel>> GetOursourceStepRequestReport(int page, int size, string orderByFieldName, bool asc, Clause filters);
        Task<IDictionary<long, DailyImportModel>> GetDailyImport(long monthPlanId, int stepId);
    }
}
