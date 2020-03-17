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
    public interface IPurchasingSuggestService
    {
        Task<ServiceResult<PurchasingSuggestOutput>> GetInfo(long PurchasingSuggestId);

        Task<PageData<PurchasingSuggestOutputList>> GetList(string keyword, EnumPurchasingSuggestStatus? PurchasingSuggestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<ServiceResult<long>> Create(PurchasingSuggestInput model);

        Task<Enum> Update(long PurchasingSuggestId, PurchasingSuggestInput model);              

        Task<Enum> Delete(long PurchasingSuggestId);

        Task<Enum> SendToCensor(long PurchasingSuggestId);

        Task<Enum> Approve(long PurchasingSuggestId);

        Task<Enum> Reject(long PurchasingSuggestId);
        Task<Enum> UpdatePoProcessStatus(long PurchasingSuggestId, EnumPoProcessStatus poProcessStatusId);

    }
}
