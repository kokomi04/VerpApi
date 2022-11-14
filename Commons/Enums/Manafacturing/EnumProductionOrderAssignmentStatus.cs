using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionOrderAssignmentStatus
    {
        [Description("Chưa phân công")]
        NoAssignment = 0,
        [Description("Hoàn thành")]
        Completed = 1,
        [Description("Đang phân công")]
        AssignProcessing = 2

    }
}
