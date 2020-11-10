﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Commons.Enums.Manafacturing;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IProductionProcessService
    {
        Task<ProductionProcessInfo> GetProductionProcessByContainerId(EnumProductionProcess.ContainerType containerTypeId, long containerId);
        Task<ProductionStepInfo> GetProductionStepById(int containerId, long productionStepId);
        Task<bool> UpdateProductionStepById(int containerId, long productionStepId, ProductionStepInfo req);
        Task<long> CreateProductionStep(int containerId, ProductionStepInfo req);
        Task<bool> DeleteProductionStepById(int containerId, long productionStepId);
        Task<bool> MergeProductionProcess(int productOrderId, IList<long> productionStepIds);
        Task<bool> CreateProductionProcess(int productionOrderId);
        //Task<bool> GenerateProductionStepMapping(int containerId, List<ProductionStepLinkModel> req);
    }
}
