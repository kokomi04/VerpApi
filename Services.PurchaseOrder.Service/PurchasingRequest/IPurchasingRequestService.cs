using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PurchasingRequest;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Services.Master.Model.Activity;

namespace VErp.Services.PurchaseOrder.Service.PurchasingRequest
{
    public interface IPurchasingRequestService
    {
        Task<ServiceResult<PurchasingRequestOutputModel>> Get(long purchasingRequestId);

        Task<PageData<PurchasingRequestOutputModel>> GetList(string keyword, IList<int> statusList, long beginTime = 0, long endTime = 0, int page = 1, int size = 10);

        Task<ServiceResult<long>> AddPurchasingRequest(int currentUserId, PurchasingRequestInputModel model);

        Task<Enum> UpdatePurchasingRequest(long purchasingRequestId, int currentUserId, PurchasingRequestInputModel model);              

        Task<Enum> DeletePurchasingRequest(long purchasingRequestId, int currentUserId);

        Task<Enum> SendToApprove(long purchasingRequestId, int currentUserId);

        Task<Enum> ApprovePurchasingRequest(long purchasingRequestId, int currentUserId);

        Task<Enum> RejectPurchasingRequest(long purchasingRequestId, int currentUserId);

        Task<Enum> AddNote(long objectId, int currentUserId, int actionTypeId = 0, string note = "");

        Task<PageData<UserActivityLogOuputModel>> GetNoteList(long objectId, int pageIndex = 1, int pageSize = 20);
    }
}
