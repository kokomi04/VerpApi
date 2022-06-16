using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionMaterialsRequirementService
    {
        Task<long> AddProductionMaterialsRequirement(ProductionMaterialsRequirementModel model, EnumProductionMaterialsRequirementStatus status);
        Task<bool> UpdateProductionMaterialsRequirement(long requirementId, ProductionMaterialsRequirementModel model);
        Task<bool> DeleteProductionMaterialsRequirement(long requirementId);
        Task<ProductionMaterialsRequirementModel> GetProductionMaterialsRequirement(long requirementId);

        Task<PageData<ProductionMaterialsRequirementDetailSearch>> SearchProductionMaterialsRequirement(long productionOrderId, string keyword, int page, int size, Clause filters);
        Task<long> ConfirmInventoryRequirement(long requirementId, EnumProductionMaterialsRequirementStatus status);
        Task<IList<ProductionMaterialsRequirementDetailListModel>> GetProductionMaterialsRequirementByProductionOrder(long productionOrderId);
    }
}
