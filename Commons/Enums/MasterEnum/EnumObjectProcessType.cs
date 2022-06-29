using System.ComponentModel;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumObjectProcessType
    {
        [Description("Nhập kho")]
        InventoryInput = EnumObjectType.InventoryInput,
        [Description("Xuất kho")]
        InventoryOutput = EnumObjectType.InventoryOutput,
        [Description("Yêu cầu vật tư")]
        PurchasingRequest = EnumObjectType.PurchasingRequest,
        [Description("Đề nghị mua hàng")]
        PurchasingSuggest = EnumObjectType.PurchasingSuggest,
        [Description("Phiếu mua hàng")]
        PurchaseOrder = EnumObjectType.PurchaseOrder,
    }
}
