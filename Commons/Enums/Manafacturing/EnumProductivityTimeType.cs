using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductivityTimeType
    {
        [Description("Ngày")]
        Day = 1,
        [Description("Giờ")]
        Hour = 2
    }

    public enum EnumProductivityResourceType
    {
        [Description("Người")]
        Human = 1,
        [Description("Máy")]
        Machine = 2
    }

    public enum EnumWorkloadType
    {
        [Description("Theo số lượng")]
        Quantity = 1,
        [Description("Theo khối lượng tinh")]
        Purity = 2
    }
}
