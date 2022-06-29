using Verp.Resources.Enums.ErrorCodes.Manufacturing;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes
{
    [LocalizedDescription(ResourceType = typeof(ProductionProcessErrorCodeDescription))]
    public enum ProductionProcessErrorCode
    {
        NotFoundProductionStep = 1,
        NotFoundInOutStep = 2,
        InvalidDeleteProductionStep = 3,
        ValidateProductionStepLinkData = 4,
        ExistsProductionProcess = 5,
        NotFoundProductionProcess = 6,
        ListProductionStepNotInContainerId = 7,
        ValidateProductionStep = 8,
        ValidateProductionStepLinkDataRole = 10,
    }
}
