﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;
using PurchaseOrderEntity = VErp.Infrastructure.EF.PurchaseOrderDB.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderService
    {
        Task<PageData<PurchaseOrderOutputList>> GetList(PurchaseOrderFilterRequestModel req);

        Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct(PurchaseOrderFilterRequestModel req);

        Task<PurchaseOrderOutput> GetInfo(long purchaseOrderId);

        Task<long> Create(PurchaseOrderInput req);

        Task<PurchaseOrderEntity> CreateToDb(PurchaseOrderInput model);

        Task<bool> Update(long purchaseOrderId, PurchaseOrderInput req);

        CategoryNameModel GetFieldDataForParseMapping();

        IAsyncEnumerable<PurchaseOrderInputDetail> ParseDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream);

        CategoryNameModel GetFieldDataForImportMapping();
        Task<bool> Import(ImportExcelMapping mapping, Stream stream);

        Task<bool> SentToCensor(long purchaseOrderId);

        Task<bool> Checked(long purchaseOrderId);

        Task<bool> RejectCheck(long purchaseOrderId);

        Task<bool> Approve(long purchaseOrderId);

        Task<bool> Reject(long purchaseOrderId);

        Task<bool> Delete(long purchaseOrderId);

        Task<bool> UpdatePoProcessStatus(long purchaseOrderId, EnumPoProcessStatus poProcessStatusId);

        Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderBySuggest(IList<long> purchasingSuggestIds);

        Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderByAssignment(IList<long> poAssignmentIds);
        Task<bool> SendMailNotifyCheckAndCensor(long purchaseOrderId, string mailTemplateCode, string[] mailTo);

        Task<IList<PurchaseOrderOutsourcePartAllocate>> GetAllPurchaseOrderOutsourcePart();
        Task<IList<EnrichDataPurchaseOrderAllocate>> EnrichDataForPurchaseOrderAllocate(long purchaseOrderId);

        Task<bool> RemoveOutsourcePart(long[] arrPurchaseOrderId, long outsourcePartRequestId);
    }
}
