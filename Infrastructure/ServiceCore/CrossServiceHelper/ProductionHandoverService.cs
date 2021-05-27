﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.Manafacturing;
using System.Data;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IProductionHandoverService
    {
        Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, DataTable inventories);
    }
    public class ProductionHandoverService : IProductionHandoverService
    {
        private readonly IHttpCrossService _httpCrossService;

        public ProductionHandoverService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, DataTable inventories)
        {
            return await _httpCrossService.Put<bool>($"api/internal/InternalProductionHandover/productionOrder/{productionOrderId}/productionStep/{productionStepId}/department/{departmentId}/status", inventories.ConvertData<ProductionInventoryRequirementModel>());
        }
    }
}
