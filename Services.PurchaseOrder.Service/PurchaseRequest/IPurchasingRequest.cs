using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using Services.PurchaseOrder.Model.PurchaseRequest;
namespace Services.PurchaseOrder.Service.PurchaseRequest
{
    interface IPurchasingRequest
    {
        Task<ServiceResult<PurchasingRequestOutputModel>> Get(long purchasingRequestId);

        Task<PageData<PurchasingRequestOutputModel>> GetList(string keyword, long beginTime = 0, long endTime = 0, int page = 1, int size = 10);

        Task<ServiceResult<long>> AddPurchasingRequest(int currentUserId, PurchasingRequestInputModel model);

        Task<Enum> UpdatePurchasingRequest(long purchasingRequestId, int currentUserId, PurchasingRequestInputModel model);

        Task<Enum> ApprovePurchasingRequest(long purchasingRequestId, int currentUserId);

        Task<Enum> DeletePurchasingRequest(long purchasingRequestId, int currentUserId);
    }
}
