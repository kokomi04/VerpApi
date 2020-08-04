using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumTimeType
    {
        [Description("Năm")]
        Year = 1,
        [Description("Tháng")]
        Month = 2,
        [Description("Tuần")]
        Week = 3,
        [Description("Ngày")]
        Day = 4,
        [Description("Giờ")]
        Hour = 5,
        [Description("Phút")]
        Minute = 6
    }
}
