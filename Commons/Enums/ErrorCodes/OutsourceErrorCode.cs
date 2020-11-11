using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("OUTS")]
    public enum OutsourceErrorCode
    {
        [Description("Không tìm thấy yêu cầu gia công")]
        NotFoundRequest = 1,
        [Description("Không tìm thấy đơn hàng gia công")]
        NotFoundOutsourOrder = 2,
        InValidRequestOutsource = 3,
    }
}
