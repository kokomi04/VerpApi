using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover
{
    public interface IProductionHistoryService
    {

        Task<IList<ProductionHistoryModel>> GetProductionHistories(long productionOrderId);
   
        Task<ProductionHistoryModel> CreateProductionHistory(long productionOrderId, ProductionHistoryInputModel data);
        Task<IList<ProductionHistoryModel>> CreateMultipleProductionHistory(long productionOrderId, IList<ProductionHistoryInputModel> data);
        Task<bool> DeleteProductionHistory(long productionHistoryId);
        Task<IDictionary<long, ActualWorkloadModel>> GetActualWorkload(long fromDate, long toDate);
    }
}
