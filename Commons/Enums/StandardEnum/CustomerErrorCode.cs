using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("CUS")]
    public enum CustomerErrorCode
    {
        [Description("Không tìm thấy đối tác")]
        CustomerNotFound = 1,
        [Description("Mã đối tác đã tồn tại")]
        CustomerCodeAlreadyExisted = 2,
        [Description("Tên đối tác đã tồn tại")]
        CustomerNameAlreadyExisted = 3,
    }
}
