using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionProcessWarningCode
    {
        [Description("Warning-Công đoạn")]
        WarningProductionStep = 1,
        [Description("Warning-Chi tiết")]
        WarningProductionLinkData = 2,
        [Description("Warning-YCGC chi tiết")]
        WarningOutsourcePartRequest = 3,
        [Description("Warning-YCGC công đoạn")]
        WarningOutsourceStepRequest = 4
    }
}
