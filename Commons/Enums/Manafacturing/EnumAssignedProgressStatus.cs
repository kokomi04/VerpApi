using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumAssignedProgressStatus : int
    {
        [Description("Đợi bàn giao")]
        Waiting = 1,
        [Description("Đang bàn giao")]
        HandingOver = 1,
        [Description("Hoàn thành")]
        Finish = 2
    }
}
