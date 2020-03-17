using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Services.Master.Model.Activity;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Services.PurchaseOrder.Model;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchasingRequestService
    {
        Task<ServiceResult<PurchasingRequestOutput>> GetInfo(long purchasingRequestId);

        Task<PageData<PurchasingRequestOutputList>> GetList(string keyword, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<ServiceResult<long>> Create(PurchasingRequestInput model);

        Task<Enum> Update(long purchasingRequestId, PurchasingRequestInput model);              

        Task<Enum> Delete(long purchasingRequestId);

        Task<Enum> SendToCensor(long purchasingRequestId);

        Task<Enum> Approve(long purchasingRequestId);

        Task<Enum> Reject(long purchasingRequestId);
        Task<Enum> UpdatePoProcessStatus(long purchasingRequestId, EnumPoProcessStatus poProcessStatusId);

    }
}
