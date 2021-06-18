using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IMaterialCalcService
    {
        Task<long> Create(MaterialCalcModel req);
        Task<MaterialCalcModel> Info(long materialCalcId);

        Task<bool> Update(long materialCalcId, MaterialCalcModel req);

        Task<bool> Delete(long materialCalcId);
    }
}
