﻿using System;
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
        Task<ServiceResult<PurchasingSuggestOutput>> GetInfo(long purchasingSuggestId);

        Task<PageData<PurchasingSuggestOutputList>> GetList(string keyword, EnumPurchasingSuggestStatus? purchasingSuggestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<ServiceResult<long>> Create(PurchasingSuggestInput model);

        Task<Enum> Update(long purchasingSuggestId, PurchasingSuggestInput model);

        Task<Enum> Delete(long purchasingSuggestId);

        Task<Enum> SendToCensor(long purchasingSuggestId);

        Task<Enum> Approve(long purchasingSuggestId);

        Task<Enum> Reject(long purchasingSuggestId);
        Task<Enum> UpdatePoProcessStatus(long purchasingSuggestId, EnumPoProcessStatus poProcessStatusId);

        Task<PageData<PoAssignmentOutputList>> PoAssignmentList(string keyword, EnumPoAssignmentStatus? poAssignmentStatusId, int? assigneeUserId, long? purchasingSuggestId, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);
        Task<ServiceResult<IList<PoAssignmentOutput>>> PoAssignmentListBySuggest(long purchasingSuggestId);

        Task<ServiceResult<long>> PoAssignmentCreate(long purchasingSuggestId, PoAssignmentInput model);

    }
}
