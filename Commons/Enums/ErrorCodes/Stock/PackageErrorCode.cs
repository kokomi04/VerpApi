using Verp.Resources.Enums.Stock;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PAK")]
    [LocalizedDescription(ResourceType = typeof(PackageErrorCodeDescription))]

    public enum PackageErrorCode
    {
        PackageNotFound = 1,
        PackageCodeEmpty = 2,
        PackageAlreadyExisted = 3,
        PackageNotAllowUpdate = 4,
        QualtityOfProductInPackageNotEnough = 5,
        HasSomeQualtityWaitingForApproved = 6,
        PackagesToJoinMustBeSameProductAndUnit = 7,
        CanNotJoinDefaultPackage = 8
    }
}
