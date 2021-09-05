using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErp.Services.Manafacturing.Service.ProductionPlan
{
    public interface IProductionPlanService
    {
        Task<IList<ProductionOrderListModel>> GetProductionOrders(long startDate, long endDate);
        Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan(long startDate, long endDate);
        Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan(IDictionary<long, List<ProductionWeekPlanModel>> data);
        Task<bool> DeleteProductionPlan(long productionOrderId);
        Task<(Stream stream, string fileName, string contentType)> ProductionPlanExport(long startDate, long endDate, ProductionPlanExportModel data, IList<string> mappingFunctionKeys = null);
    }
}
