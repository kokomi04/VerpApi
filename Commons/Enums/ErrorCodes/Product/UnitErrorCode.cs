using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("UNI")]
    public enum UnitErrorCode
    {
        EmptyUnitName = 1,
        UnitNotFound = 2,
        UnitNameAlreadyExisted = 3
    }
}
