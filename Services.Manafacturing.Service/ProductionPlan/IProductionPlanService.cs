using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionPlan;
using VErp.Services.Manafacturing.Model.WorkloadPlanModel;
using ProductSemiEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;

namespace VErp.Services.Manafacturing.Service.ProductionPlan
{
    public interface IProductionPlanService
    {
        Task<IList<ProductionOrderListModel>> GetProductionPlans(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate);
        Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate);
        Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan(IDictionary<long, List<ProductionWeekPlanModel>> data);
        Task<bool> DeleteProductionPlan(long productionOrderId);
        Task<(Stream stream, string fileName, string contentType)> ProductionPlanExport(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate, ProductionPlanExportModel data, IList<string> mappingFunctionKeys = null);

        Task<IDictionary<long, WorkloadPlanModel>> GetWorkloadPlan(IList<long> productionOrderIds);
        Task<IDictionary<long, WorkloadPlanModel>> GetWorkloadPlanByDate(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate);

        Task<IDictionary<long, List<ImportProductModel>>> GetMonthlyImportStock(int monthPlanId);
        List<ProductSemiEntity> GetProductSemis(List<long> productSemiIds);
        Task<(Stream stream, string fileName, string contentType)> ProductionWorkloadPlanExport(int monthPlanId, int? factoryDepartmentId, long startDate, long endDate, string monthPlanName, IList<string> mappingFunctionKeys = null);

    }
}
