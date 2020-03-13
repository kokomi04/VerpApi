using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PurchasingSuggest;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Services.Master.Model.Activity;

namespace VErp.Services.PurchaseOrder.Service.PurchasingSuggest
{
    public interface IPurchasingSuggestService
    {
        Task<ServiceResult<PurchasingSuggestOutputModel>> Get(long purchasingSuggestId);

        Task<PageData<PurchasingSuggestOutputModel>> GetList(string keyword, IList<EnumPurchasingSuggestStatus> statusList, long beginTime = 0, long endTime = 0, int page = 1, int size = 10);

        Task<ServiceResult<long>> AddPurchasingSuggest(int currentUserId, PurchasingSuggestInputModel model);

        Task<Enum> UpdatePurchasingSuggest(long purchasingSuggestId, int currentUserId, PurchasingSuggestInputModel model);              

        Task<Enum> DeletePurchasingSuggest(long purchasingSuggestId, int currentUserId);

        Task<Enum> SendToApprove(long purchasingSuggestId, int currentUserId);

        Task<Enum> ApprovePurchasingSuggest(long purchasingSuggestId, int currentUserId);

        Task<Enum> RejectPurchasingSuggest(long purchasingSuggestId, int currentUserId);

        Task<Enum> AddNote(long objectId, int currentUserId, int actionTypeId = 0, string note = "");

        Task<PageData<UserActivityLogOuputModel>> GetNoteList(long objectId, int pageIndex = 1, int pageSize = 20);
    }
}
