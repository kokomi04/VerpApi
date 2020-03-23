using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{

    [ErrorCodePrefix("BI")]
    public enum BusinessInfoErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        BusinessInfoNotFound = 1,
    }
}
