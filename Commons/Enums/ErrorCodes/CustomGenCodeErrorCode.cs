using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{

    [ErrorCodePrefix("CGC")]
    public enum CustomGenCodeErrorCode
    {
        ConfigAlreadyExisted = 1,
        ConfigNotFound,
        EmptyConfig,
        AllowOnlyOneConfigActivedAtTheSameTime,
        NoActivedConfigWasFound,
        NotSupportedYet
    }
}
