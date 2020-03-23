using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("LOC")]
    public enum LocationErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        LocationNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        LocationCodeEmpty = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        LocationAlreadyExisted = 3
    }
}
