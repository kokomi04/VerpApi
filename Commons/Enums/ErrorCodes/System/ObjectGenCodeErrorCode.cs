namespace VErp.Commons.Enums.StandardEnum
{

    [ErrorCodePrefix("OGC")]
    public enum ObjectGenCodeErrorCode
    {
        ConfigAlreadyExisted = 1,
        ConfigNotFound,
        EmptyConfig,
        AllowOnlyOneConfigActivedAtTheSameTime,
        NoActivedConfigWasFound,
        NotSupportedYet
    }
}
