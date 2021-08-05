using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderOutsourceStepService
    {
        Task<long> CreatePurchaseOrderOutsourceStep(PurchaseOrderInput model);
        Task<bool> DeletePurchaseOrderOutsourceStep(long purchaseOrderId);
        Task<IList<RefOutsourceStepRequestModel>> GetOutsourceStepRequest();
        Task<IList<RefOutsourceStepRequestModel>> GetOutsourceStepRequest(long[] arrOutsourceStepId);
        Task<bool> UpdatePurchaseOrderOutsourceStep(long purchaseOrderId, PurchaseOrderInput model);
    }

}
