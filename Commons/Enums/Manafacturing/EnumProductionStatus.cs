using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionStatus : int
    {
        [Description("Chưa sản xuât")]
        Waiting = 1,
        [Description("Đang sản xuất")]
        Processing = 2,
        [Description("Hoàn thành")]
        Finished = 3
    }
}
