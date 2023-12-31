﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IValidateProductionOrderService
    {
        Task<IList<string>> ValidateProductionOrder(long productionOrderId);
        Task<IList<string>> GetWarningProductionOrder(long productionOrderId, IEnumerable<ProductionOrderDetailOutputModel> productionOrderDetail);
    }
}
