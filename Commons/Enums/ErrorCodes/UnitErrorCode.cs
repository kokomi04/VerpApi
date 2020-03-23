using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("UNI")]
    public enum UnitErrorCode
    {
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        EmptyUnitName = 1,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        UnitNotFound = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        UnitNameAlreadyExisted = 3
    }
}
