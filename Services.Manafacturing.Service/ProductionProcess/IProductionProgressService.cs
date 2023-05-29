using System.Threading.Tasks;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IProductionProgressService
    {
        Task<bool> CalcAndUpdateProductionOrderStatus(ProductionOrderCalcStatusMessage data);
    }
}
