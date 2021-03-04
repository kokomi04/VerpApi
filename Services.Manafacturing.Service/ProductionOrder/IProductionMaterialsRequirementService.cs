using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Service.ProductionOrder
{
    public interface IProductionMaterialsRequirementService
    {
        Task<long> AddProductionMaterialsRequirement(ProductionMaterialsRequirementModel model);
        Task<bool> UpdateProductionMaterialsRequirement(long requirementId, ProductionMaterialsRequirementModel model);
        Task<bool> DeleteProductionMaterialsRequirement(long requirementId);
        Task<ProductionMaterialsRequirementModel> GetProductionMaterialsRequirement(long requirementId);

        Task<PageData<ProductionMaterialsRequirementDetailSearch>> SearchProductionMaterialsRequirement(string keyword, int page, int size, Clause filters);
        Task<bool> ConfirmInventoryRequirement(long requirementId, EnumProductionMaterialsRequirementStatus status);
    }
}
