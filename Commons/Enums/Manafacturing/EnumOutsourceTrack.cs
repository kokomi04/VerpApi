using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public static class EnumOutsourceTrack
    {
        public enum EnumOutsourceTrackType: int
        {
            [Description("Cập nhật toàn bộ chi tiết")]
            All = 1,
            [Description("Cập nhật theo chi tiết")]
            Detail = 2,
        }

        public enum EnumOutsourceTrackStatus : int
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
}
