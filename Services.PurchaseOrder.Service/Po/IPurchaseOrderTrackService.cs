using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderTrackService
    {
        Task<long> CreatePurchaseOrderTrack(long purchaseOrderId, purchaseOrderTrackedModel req);
        Task<bool> DeletePurchaseOrderTrack(long purchaseOrderId, long PurchaseOrderTrackId);
        Task<IList<purchaseOrderTrackedModel>> SearchPurchaseOrderTrackByPurchaseOrder(long purchaseOrderId);
        Task<bool> UpdatePurchaseOrderTrack(long purchaseOrderId, long PurchaseOrderTrackId, purchaseOrderTrackedModel req);
        Task<bool> UpdatePurchaseOrderTrackByPurchaseOrderId(long purchaseOrderId, IList<purchaseOrderTrackedModel> req);
    }
}