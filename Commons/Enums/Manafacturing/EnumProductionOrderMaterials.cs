using System.ComponentModel;

namespace VErp.Commons.Enums.Manafacturing
{
    public static class EnumProductionOrderMaterials
    {
        public enum EnumInventoryRequirementStatus
        {
            [Description("Chưa tạo")]
            NotCreateYet = 1,
            [Description("Đã tạo")]
            Created = 2,
        }
    }
}
