﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service
{
    public interface IMaterialCalcService
    {
        Task<PageData<MaterialCalcListModel>> GetList(string keyword, Clause filter, int page, int size);

        Task<long> Create(MaterialCalcModel req);
        Task<MaterialCalcModel> Info(long materialCalcId);

        Task<bool> Update(long materialCalcId, MaterialCalcModel req);

        Task<bool> Delete(long materialCalcId);
    }
}
