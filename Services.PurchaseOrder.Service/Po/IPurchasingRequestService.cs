using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.Request;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPurchasingRequestService
    {
        Task<PurchasingRequestOutput> GetInfo(long purchasingRequestId);

        Task<PurchasingRequestOutput> GetByOrderDetailId(long orderDetailId);

        Task<PageData<PurchasingRequestOutputList>> GetList(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<PageData<PurchasingRequestOutputListByProduct>> GetListByProduct(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);

        Task<IList<PurchasingRequestDetailInfo>> PurchasingRequestDetailInfo(IList<long> purchasingRequestDetailIds);

        Task<long> Create(EnumPurchasingRequestType requestType, PurchasingRequestInput model);

        CategoryNameModel GetFieldDataForMapping();

        IAsyncEnumerable<PurchasingRequestInputDetail> ParseDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream);

        Task<bool> Update(EnumPurchasingRequestType purchasingRequestTypeId, long purchasingRequestId, PurchasingRequestInput model);

        Task<bool> Delete(long? orderDetailId, long? materialCalcId, long? productionOrderId, long purchasingRequestId);

        Task<bool> SendToCensor(long purchasingRequestId);

        Task<bool> Approve(long purchasingRequestId);

        Task<bool> Reject(long purchasingRequestId);
        Task<bool> UpdatePoProcessStatus(long purchasingRequestId, EnumPoProcessStatus poProcessStatusId);

        Task<IList<PurchasingRequestOutput>> GetPurchasingRequestByProductionOrder(long productionOrderId, int? productMaterialsConsumptionGroupId, long? productionOrderMaterialSetId);

    }
}
