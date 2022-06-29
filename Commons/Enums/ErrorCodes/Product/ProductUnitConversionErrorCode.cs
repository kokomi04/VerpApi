using Verp.Resources.Enums.ErrorCodes.Product;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CONV")]
    [LocalizedDescription(ResourceType = typeof(ProductUnitConversionErrorCodeDescription))]

    public enum ProductUnitConversionErrorCode
    {
        ProductUnitConversionNotFound = 1,
        ProductSecondaryUnitAlreadyExisted = 2,
        SecondaryUnitConversionError = 3,
        PrimaryUnitConversionError = 4,
        ProductUnitConversionNotBelongToProduct = 5
    }
}
