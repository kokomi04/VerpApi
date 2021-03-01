using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StockEnum
{
    public enum EnumInventoryOutsideMappingType
    {
        [Description("Hoàn chỉnh")]
        Normal = 0,
        [Description("Đơn hàng")]
        Order = 1,
        [Description("Lệnh sản xuất")]
        ProductionOrder = 2,
        [Description("Yêu cầu gia công")]
        OutsourceRequest = 3,
    }
}
