﻿using System.ComponentModel;

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
        WarningOutsourceStepRequest = 4,
        [Description("Warning-Lệnh sản xuất")]
        WarningProductionOrder = 5,
        [Description("Warning-Đơn hàng")]
        WarningProduct = 6
    }
}
