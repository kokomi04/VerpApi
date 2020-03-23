using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CUS")]
    public enum CustomerErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Không tìm thấy đối tác")]
        CustomerNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Mã đối tác đã tồn tại")]
        CustomerCodeAlreadyExisted = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tên đối tác đã tồn tại")]
        CustomerNameAlreadyExisted = 3,
    }
}
