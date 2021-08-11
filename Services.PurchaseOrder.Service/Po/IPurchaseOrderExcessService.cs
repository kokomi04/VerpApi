using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderExcessService
    {
        Task<bool> UpdatePurchaseOrderExcess(long purchaseOrderExcessId, PurchaseOrderExcessModel model);
    }
}