using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("STK")]
    public enum StockErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        StockNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        StockCodeEmpty = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        StockCodeAlreadyExisted = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        StockNameAlreadyExisted = 4,
    }
}
