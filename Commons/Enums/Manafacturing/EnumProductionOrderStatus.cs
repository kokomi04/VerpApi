using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionOrderStatus : int
    {
        [Description("Bản nháp")]
        Draft = 1,
        [Description("Chưa sản xuât")]
        Waiting = 2,
        [Description("Đang sản xuất")]
        Processing = 3,
        [Description("Hoàn thành")]
        Finished = 4
    }
}
