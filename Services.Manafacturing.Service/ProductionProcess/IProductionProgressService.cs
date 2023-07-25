using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IProductionProgressService
    {
        //Task<bool> CalcAndUpdateProductionOrderStatus(ProductionOrderCalcStatusMessage data);
        bool IsPendingCalcStatus(string producionOrderCode);
        Task<IList<ProductionOrderInventoryConflictModel>> GetConflictInventories(long productionOrderId);
        Task<bool> CalcAndUpdateProductionOrderStatusV2(ProductionOrderCalcStatusV2Message data);
    }
}
