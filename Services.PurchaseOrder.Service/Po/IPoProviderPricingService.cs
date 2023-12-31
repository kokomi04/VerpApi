﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.PO;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PoProviderPricing;

namespace VErp.Services.PurchaseOrder.Service.Po
{
    public interface IPoProviderPricingService
    {
        Task<bool> Approve(long purchaseOrderId);
        Task<bool> Checked(long purchaseOrderId);
        Task<long> Create(PoProviderPricingModel model);
        Task<bool> Delete(long poProviderPricingId);
        Task<PoProviderPricingModel> GetInfo(long poProviderPricingId);
        Task<PageData<PoProviderPricingOutputList>> GetList(string keyword, int? customerId, IList<int> productIds, EnumPoProviderPricingStatus? poProviderPricingStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);
        Task<PageData<PoProviderPricingOutputListByProduct>> GetListByProduct(string keyword, int? customerId, IList<string> codes, IList<int> productIds, EnumPoProviderPricingStatus? poProviderPricingStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size);
        Task<bool> Reject(long purchaseOrderId);
        Task<bool> RejectCheck(long purchaseOrderId);
        Task<bool> SentToCensor(long purchaseOrderId);
        Task<bool> Update(long poProviderPricingId, PoProviderPricingModel model);
        Task<bool> UpdatePoProcessStatus(long purchaseOrderId, EnumPoProcessStatus poProcessStatusId);

        CategoryNameModel GetFieldDataForMapping();

        IAsyncEnumerable<PoProviderPricingOutputDetail> ParseDetails(ImportExcelMapping mapping, Stream stream);
    }
}
