using Verp.Resources.Enums.ErrorCodes.PO;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes
{
    [LocalizedDescription(ResourceType = typeof(PurchasingRequestErrorCodeDescription))]
    public enum PurchasingRequestErrorCode
    {
        RequestNotFound = 1,
        RequestCodeEmpty = 2,
        RequestCodeAlreadyExisted = 3,

        //RequestAlreadyApproved = 4,
    }
}
