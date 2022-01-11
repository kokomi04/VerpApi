using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderOutsourcePartService
    {
        Task<long> CreatePurchaseOrderOutsourcePart(PurchaseOrderInput model);
        Task<bool> DeletePurchaseOrderOutsourcePart(long purchaseOrderId);
        Task<IList<RefOutsourcePartRequestModel>> GetOutsourcePartRequest(long[] arrOutsourcePartId, string productionOrderCode, int? productId);
        Task<bool> UpdatePurchaseOrderOutsourcePart(long purchaseOrderId, PurchaseOrderInput model);
        Task<PurchaseOrderOutput> GetPurchaseOrderOutsourcePart(long purchaseOrderId);
        Task<bool> UpdateStatusForOutsourceRequestInPurcharOrder(long purchaseOrderId);
    }

}
