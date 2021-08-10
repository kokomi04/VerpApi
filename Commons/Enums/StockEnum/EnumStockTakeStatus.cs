using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Stock
{
    public enum EnumStockTakeStatus : int
    {
        [Description("Đang chờ")]
        Waiting = 1,
        [Description("Đang thực hiện")]
        Processing = 2,
        [Description("Hoàn thành")]
        Finish = 3,
    }
}
