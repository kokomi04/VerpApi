using Verp.Resources.Enums.ErrorCodes.Organization;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CUS")]
    [LocalizedDescription(ResourceType = typeof(CustomerErrorCodeDescription))]
    public enum CustomerErrorCode
    {
        CustomerNotFound = 1,
        CustomerCodeAlreadyExisted = 2,
        CustomerNameAlreadyExisted = 3,
        CustomerInUsed = 4
    }
}
