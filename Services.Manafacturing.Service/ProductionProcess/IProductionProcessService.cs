using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Commons.Enums.Manafacturing;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionProcess
{
    public interface IProductionProcessService
    {
        Task<bool> UpdateProductionProcess(EnumContainerType containerTypeId, long containerId, ProductionProcessModel req);

        Task<ProductionProcessInfo> GetProductionProcessByScheduleTurn(long scheduleTurnId);
        Task<ProductionProcessModel> GetProductionProcessByContainerId(EnumContainerType containerTypeId, long containerId);
        Task<ProductionStepInfo> GetProductionStepById(long productionStepId);
        Task<bool> UpdateProductionStepById(long productionStepId, ProductionStepInfo req);
        Task<long> CreateProductionStep(ProductionStepInfo req);
        Task<bool> DeleteProductionStepById(long productionStepId);
        Task<bool> MergeProductionProcess(int productOrderId, IList<long> productionStepIds);
        Task<bool> IncludeProductionProcess(int productionOrderId);
        Task<bool> MergeProductionStep(int productionOrderId, IList<long> productionStepIds);
        //Task<bool> GenerateProductionStepMapping(int containerId, List<ProductionStepLinkModel> req);

        Task<bool> UpdateProductionStepSortOrder(IList<PorductionStepSortOrderModel> req);

        //ProductionStepRoleClient
        Task<bool> InsertAndUpdatePorductionStepRoleClient(ProductionStepRoleClientModel  model);
        Task<string> GetPorductionStepRoleClient(int containerTypeId, long containerId);
        // StepGroup
        Task<long> CreateProductionStepGroup(ProductionStepGroupModel req);

        //ProductionStepLinkData
        Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId(List<long> lsProductionStepId);
        Task<IList<ProductionStepLinkDataRoleModel>> GetListStepLinkDataForOutsourceStep(List<long> lsProductionStepId);
    }
}
