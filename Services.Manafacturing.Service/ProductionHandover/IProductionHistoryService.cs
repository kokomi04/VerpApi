using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IProductionHistoryService
    {

        Task<IList<ProductionHistoryModel>> GetProductionHistories(long productionOrderId);

        Task<ProductionHistoryModel> CreateProductionHistory(long productionOrderId, ProductionHistoryInputModel data);
        Task<IList<ProductionHistoryModel>> CreateMultipleProductionHistory(long productionOrderId, IList<ProductionHistoryInputModel> data);
        Task<bool> DeleteProductionHistory(long productionHistoryId);
        Task<IDictionary<long, ActualWorkloadModel>> GetActualWorkloadByDate(long fromDate, long toDate);
        Task<IDictionary<long, ActualWorkloadModel>> GetCompletionActualWorkload(long fromDate, long toDate);
    }
}
