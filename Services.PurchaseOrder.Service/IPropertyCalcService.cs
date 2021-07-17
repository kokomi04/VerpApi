using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IPropertyCalcService
    {
        Task<PageData<PropertyCalcListModel>> GetList(string keyword, ArrayClause filter, int page, int size);

        IAsyncEnumerable<PropertyOrderProductHistory> GetHistoryProductOrderList(IList<int> productIds, IList<string> orderCodes);

        Task<long> Create(PropertyCalcModel req);
        Task<PropertyCalcModel> Info(long propertyCalcId);

        Task<bool> Update(long propertyCalcId, PropertyCalcModel req);

        Task<bool> Delete(long propertyCalcId);

        Task<IList<CuttingWorkSheetModel>> GetCuttingWorkSheet(long propertyCalcId);

        Task<IList<CuttingWorkSheetModel>> UpdateCuttingWorkSheet(long propertyCalcId, IList<CuttingWorkSheetModel> data);
    }
}
