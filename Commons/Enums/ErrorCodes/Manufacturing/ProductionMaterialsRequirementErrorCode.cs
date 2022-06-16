using Verp.Resources.Enums.ErrorCodes.Manufacturing;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes
{
    [LocalizedDescription(ResourceType = typeof(ProductionMaterialsRequirementErrorCodeDescription))]
    public enum ProductionMaterialsRequirementErrorCode
    {
        OutsoureOrderCodeAlreadyExisted = 1,
        NotFoundRequirement = 2,
        NotFoundDetailMaterials = 3
    }
}
