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
        Task<ProductionProcessInfo> GetProductionProcessByScheduleTurn(long scheduleTurnId);
        Task<ProductionProcessInfo> GetProductionProcessByContainerId(EnumProductionProcess.ContainerType containerTypeId, long containerId);
        Task<ProductionStepInfo> GetProductionStepById(long productionStepId);
        Task<bool> UpdateProductionStepById(long productionStepId, ProductionStepInfo req);
        Task<long> CreateProductionStep(ProductionStepInfo req);
        Task<bool> DeleteProductionStepById(long productionStepId);
        Task<bool> MergeProductionProcess(int productOrderId, IList<long> productionStepIds);
        Task<bool> IncludeProductionProcess(int productionOrderId);
        Task<bool> MergeProductionStep(int productionOrderId, IList<long> productionStepIds);
        //Task<bool> GenerateProductionStepMapping(int containerId, List<ProductionStepLinkModel> req);

        //ProductionStepRoleClient
        Task<bool> InsertAndUpdatePorductionStepRoleClient(ProductionStepRoleClientModel  model);
        Task<string> GetPorductionStepRoleClient(int containerTypeId, long containerId);

        Task<long> CreateProductionStepGroup(ProductionStepGroupModel req);
    }
}
