using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("BAR")]
    public enum BarcodeConfigErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        BarcodeNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        EmptyConfig = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        OnlyAllowOneBarcodeConfigActivedAtTheSameTime = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        NoActivedConfigWasFound = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        BarcodeStandardNotSupportedYet = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        BarcodeConfigHasBeenDisabled = 6
    }
}
