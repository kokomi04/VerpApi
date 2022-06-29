using System.ComponentModel;

namespace VErp.Commons.Enums.StockEnum
{
    public enum EnumInventoryOutsideMappingType
    {
        [Description("Mặc định")]
        Normal = 0,
        [Description("Đơn hàng")]
        Order = 1,
        [Description("Lệnh sản xuất")]
        ProductionOrder = 2,
        [Description("Yêu cầu gia công")]
        OutsourceRequest = 3,
    }
}
