namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("UNI")]
    public enum UnitErrorCode
    {
        EmptyUnitName = 1,
        UnitNotFound = 2,
        UnitNameAlreadyExisted = 3,
        UnitIsUnUsed = 4
    }
}
