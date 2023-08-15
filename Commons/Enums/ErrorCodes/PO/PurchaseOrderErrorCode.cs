using Verp.Resources.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes.PO
{
    [ErrorCodePrefix("PO")]
    [LocalizedDescription(ResourceType = typeof(PurchaseOrderErrorCodeDescription))]

    public enum PurchaseOrderErrorCode
    {
        PoNotFound = 1,

        PoCodeAlreadyExisted = 2,

        AssignmentDetailAlreadyCreatedPo = 3,

        OnlyCreatePOFromOneCustomer = 4,


        NotExistsOutsourceRequestId = 6,

        PrimaryQuanityGreaterThanQuantityRequirment = 7,

        ExcessNotFound = 8,

        PrimaryQuantityLessThanAllocateQuantity = 9,
    }
}
