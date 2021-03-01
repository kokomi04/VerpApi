using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StockEnum
{
    public enum EnumInventoryRequirementType
    {
        [Description("Mặc định")]
        Normal = 0,
        [Description("Hoàn chỉnh")]
        Complete = 1,
        [Description("Bổ sung")]
        Additional = 2,
    }
}
