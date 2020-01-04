namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CONV")]
    public enum ProductUnitConversionErrorCode
    {
        ProductUnitConversionNotFound = 1,
        ProductSecondaryUnitAlreadyExisted = 2,
        SecondaryUnitConversionError = 3
    }
}
