using Verp.Resources.Enums.ErrorCodes.PO;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes
{
    [LocalizedDescription(ResourceType = typeof(PurchasingSuggestErrorCodeDescription))]

    public enum PurchasingSuggestErrorCode
    {
        SuggestNotFound = 1,
        SuggestCodeEmpty = 2,
        SuggestCodeAlreadyExisted = 3,
        SuggestAlreadyApproved = 4,
        PoAssignmentCodeAlreadyExisted = 5,
        SuggestDetailNotfound = 6,
        PoAssignmentOverload = 6,
        PoAssignmentNotfound = 7,
        PoAssignmentNotEmpty = 8,
        PoAssignmentDetailNotEmpty = 9,
        PurchaseOrderDetailNotEmpty = 10,
        CanNotRejectSuggestInUse = 11,
        PurchasingSuggestIsNotApprovedYet = 12,
        PoAssignmentConfirmInvalidCurrentUser = 13,
    }
}
