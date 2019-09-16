using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("GEN")]
    public enum GeneralCode
    {
        Success = 0,
        InvalidParams = 2,
        InternalError = 3
    }
}
