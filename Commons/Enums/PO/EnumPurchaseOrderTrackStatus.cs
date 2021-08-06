using System.ComponentModel;

namespace VErp.Commons.Enums.MasterEnum.PO
{
    public enum EnumPurchaseOrderTrackStatus
    {
        [Description("Tạo đơn hàng gia công")]
        Created = 1,
        [Description("Nhà cung cấp nhận đơn")]
        Accepted = 2,
        [Description("Đang sản xuất")]
        Processing = 3,
        [Description("Đã hoàn thành")]
        Completed = 4,
        [Description("Đã bàn giao")]
        HandedOver = 5,
    }
}
