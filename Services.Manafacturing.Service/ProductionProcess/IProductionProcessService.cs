using System;
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
        Task<ProductionStepInfo> GetProductionStepById(long containerId, long productionStepId);
        Task<bool> UpdateProductionStepById(long containerId, long productionStepId, ProductionStepInfo req);
        Task<long> CreateProductionStep(long containerId, ProductionStepInfo req);
        Task<bool> DeleteProductionStepById(long containerId, long productionStepId);
        //Task<bool> GenerateProductionStepMapping(int containerId, List<ProductionStepLinkModel> req);
    }
}
