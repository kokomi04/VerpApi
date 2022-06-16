namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("LOC")]
    public enum LocationErrorCode
    {
        LocationNotFound = 1,
        LocationCodeEmpty = 2,
        LocationAlreadyExisted = 3
    }
}
