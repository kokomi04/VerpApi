﻿using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumAssignedProgressStatus : int
    {
        [Description("Đợi bàn giao")]
        Waiting = 1,
        [Description("Đang bàn giao")]
        HandingOver = 2,
        [Description("Hoàn thành")]
        Finish = 3
    }
}
