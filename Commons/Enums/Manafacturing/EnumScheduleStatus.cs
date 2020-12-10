using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumScheduleStatus : int
    {
        [Description("Chờ sản xuât")]
        Waiting = 1,
        [Description("Đang sản xuất")]
        Processing = 2,
        [Description("Hoàn thành")]
        Finished = 3,

        [Description("Chậm tiến độ")]
        OverDeadline = 4
    }
}
