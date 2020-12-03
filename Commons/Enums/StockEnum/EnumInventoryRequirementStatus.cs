using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Stock
{
    public enum EnumInventoryRequirementStatus : int
    {
        [Description("Đang chờ duyệt")]
        Waiting = 1,
        [Description("Đã duyệt")]
        Accepted = 2,
        [Description("Từ chối")]
        Rejected = 3,
    }
}
