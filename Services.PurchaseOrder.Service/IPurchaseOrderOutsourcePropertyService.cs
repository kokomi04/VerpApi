using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
   public interface IPurchaseOrderOutsourcePropertyService
    {
        Task<long> CreatePurchaseOrderOutsourceProperty(PurchaseOrderInput model);
        Task<bool> DeletePurchaseOrderOutsourceProperty(long purchaseOrderId);
        Task<PurchaseOrderOutput> GetPurchaseOrderOutsourceProperty(long purchaseOrderId);
        Task<bool> UpdatePurchaseOrderOutsourceProperty(long purchaseOrderId, PurchaseOrderInput model);
    }

}
