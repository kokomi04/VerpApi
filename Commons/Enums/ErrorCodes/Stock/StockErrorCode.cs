namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("STK")]
    public enum StockErrorCode
    {
        StockNotFound = 1,
        StockCodeEmpty = 2,
        StockCodeAlreadyExisted = 3,
        StockNameAlreadyExisted = 4,
    }
}
