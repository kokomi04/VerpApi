using System.ComponentModel;

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
