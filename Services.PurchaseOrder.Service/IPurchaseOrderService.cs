using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderService
    {
        Task<PageData<PurchaseOrderOutputList>> GetList(string keyword, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<ServiceResult<PurchaseOrderOutput>> GetInfo(long purchaseOrderId);

        Task<ServiceResult<long>> Create(PurchaseOrderInput req);

        Task<ServiceResult> Update(long purchaseOrderId, PurchaseOrderInput req);

        Task<ServiceResult> SentToCensor(long purchaseOrderId);

        Task<ServiceResult> Approve(long purchaseOrderId);

        Task<ServiceResult> Reject(long purchaseOrderId);

        Task<ServiceResult> Delete(long purchaseOrderId);

        Task<ServiceResult> UpdatePoProcessStatus(long purchaseOrderId, EnumPoProcessStatus poProcessStatusId);

    }
}
