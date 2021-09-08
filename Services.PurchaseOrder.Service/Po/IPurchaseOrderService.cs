using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchaseOrderService
    {
        Task<PageData<PurchaseOrderOutputList>> GetList(string keyword, IList<int> purchaseOrderTypes, IList<int> productIds, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct(string keyword, IList<int> purchaseOrderTypes, IList<int> productIds, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<PurchaseOrderOutput> GetInfo(long purchaseOrderId);

        Task<long> Create(PurchaseOrderInput req);

        Task<bool> Update(long purchaseOrderId, PurchaseOrderInput req);

        CategoryNameModel GetFieldDataForMapping();

        IAsyncEnumerable<PurchaseOrderExcelParseDetail> ParseInvoiceDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream);

        Task<bool> SentToCensor(long purchaseOrderId);

        Task<bool> Checked(long purchaseOrderId);

        Task<bool> RejectCheck(long purchaseOrderId);

        Task<bool> Approve(long purchaseOrderId);

        Task<bool> Reject(long purchaseOrderId);

        Task<bool> Delete(long purchaseOrderId);

        Task<bool> UpdatePoProcessStatus(long purchaseOrderId, EnumPoProcessStatus poProcessStatusId);

        Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderBySuggest(IList<long> purchasingSuggestIds);

        Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderByAssignment(IList<long> poAssignmentIds);
    }
}
