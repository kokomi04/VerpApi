using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PackingList;

namespace VErp.Services.PurchaseOrder.Service.PackingList
{
    public interface IPackingListService
    {
        Task<PageData<PackingListModel>> GetPackingLists(long voucherBillId, string keyword, int page, int size);
        Task<PackingListModel> GetPackingListById(int packingListId);
        Task<int> CreatePackingList(long voucherBillId, PackingListModel packingList);
        Task<bool> UpdatePackingList(long voucherBillId, int packingListId, PackingListModel packingList);
        Task<bool> DeletePackingList(int packingListId);
        Task<List<NonCamelCaseDictionary>> GetPackingListProductInVoucherBill(long voucherBillId);
    }
}
