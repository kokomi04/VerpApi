using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{

    [ErrorCodePrefix("OGC")]
    public enum ObjectGenCodeErrorCode
    {
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        ConfigAlreadyExisted = 1,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        ConfigNotFound,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        EmptyConfig,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        AllowOnlyOneConfigActivedAtTheSameTime,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        NoActivedConfigWasFound,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        NotSupportedYet
    }
}
