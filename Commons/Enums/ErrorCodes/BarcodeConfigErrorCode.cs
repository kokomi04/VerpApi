using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("BAR")]
    public enum BarcodeConfigErrorCode
    {
        BarcodeNotFound = 1,
        EmptyConfig = 2,
        OnlyAllowOneBarcodeConfigActivedAtTheSameTime = 3,
        NoActivedConfigWasFound = 4,
        BarcodeStandardNotSupportedYet = 5,
        BarcodeConfigHasBeenDisabled = 6
    }
}
