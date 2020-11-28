using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumHandoverStatus : int
    {
        [Description("Đợi bàn giao")]
        Waiting = 0,
        [Description("Đã duyệt")]
        Accept = 1,
        [Description("Từ chối")]
        Reject = 2
    }
}
