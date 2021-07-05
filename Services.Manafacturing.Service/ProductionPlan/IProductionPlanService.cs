using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan
{
    public interface IProductionPlanService
    {
        Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan(long startDate, long endDate);
        Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan(IDictionary<long, List<ProductionWeekPlanModel>> data);
        Task<bool> DeleteProductionPlan(long productionOrderId);
    }
}
