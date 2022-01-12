using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderOutsourceMappingService
    {
        Task<long> AddPurchaseOrderOutsourceMapping(PurchaseOrderOutsourceMappingModel model);
        Task<bool> DeletePurchaseOrderOutsourceMapping(long purchaseOrderOutsourceMappingId);
        Task<IList<PurchaseOrderOutsourceMappingModel>> GetAllByProductionOrderCode(string productionOrderCode);
        Task<IList<PurchaseOrderOutsourceMappingModel>> GetAllByPurchaseOrderId(long purchaseOrderDetailId);
        Task<bool> ImplicitAddPurchaseOrderOutsourceMappingFromManufacturing(long outsourcePartRequestId, IList<PurchaseOrderOutsourceMappingModel> models);
        Task<bool> UpdatePurchaseOrderOutsourceMapping(PurchaseOrderOutsourceMappingModel model, long purchaseOrderOutsourceMappingId);
    }
}