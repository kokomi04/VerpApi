using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IPurchaseOrderHelperService
    {
        Task<bool> RemoveOutsourcePart(long[] arrPurchaseOrderId, long outsourcePartRequestId);
    }

    public class PurchaseOrderHelperService : IPurchaseOrderHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public PurchaseOrderHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> RemoveOutsourcePart(long[] arrPurchaseOrderId, long outsourcePartRequestId)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalPurchaseOrder/outsourcePart/{outsourcePartRequestId}/removeFromPO", arrPurchaseOrderId);
        }
    }
}