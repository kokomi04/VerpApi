using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPropertyCalcService
    {
        Task<PageData<PropertyCalcListModel>> GetList(string keyword, ArrayClause filter, int page, int size, string sortBy, bool? asc);

        IAsyncEnumerable<PropertyOrderProductHistory> GetHistoryProductOrderList(IList<int> productIds, IList<string> orderCodes);

        Task<long> Create(PropertyCalcModel req);
        Task<PropertyCalcModel> Info(long propertyCalcId);

        Task<bool> Update(long propertyCalcId, PropertyCalcModel req);

        Task<bool> Delete(long propertyCalcId);

    }
}
