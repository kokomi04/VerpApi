using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionInventoryRequirementStatus : int
    {
        [Description("Đang chờ")]
        Waiting = 1,
        [Description("Đã nhận")]
        Accepted = 2,
        [Description("Bị từ chối")]
        Rejected = 3
    }
}
