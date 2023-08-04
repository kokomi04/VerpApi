using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderOrderMappingService
    {
        Task<long> AddPurchaseOrderOrderMapping(PurchaseOrderOrderMappingModel model);
        Task<bool> DeletePurchaseOrderOrderMapping(long purchaseOrderOrderMappingId);
        Task<IList<PurchaseOrderOrderMappingModel>> GetAllByPurchaseOrderDetailId(long purchaseOrderDetailId);
        Task<bool> UpdatePurchaseOrderOrderMapping(PurchaseOrderOrderMappingModel model, long purchaseOrderOrderMappingId);
    }
}