﻿using System.ComponentModel;

namespace VErp.Commons.Enums.Stock
{
    public enum EnumStockTakeStatus : int
    {
        [Description("Đang thực hiện")]
        Processing = 1,
        [Description("Hoàn thành")]
        Finish = 2,
    }
    public enum EnumStockTakePeriodStatus : int
    {
        [Description("Đang chờ")]
        Waiting = 1,
        [Description("Đang thực hiện")]
        Processing = 2,
        [Description("Hoàn thành kiểm kê")]
        Finish = 3,
        [Description("Hoàn thành xử lý")]
        FinishAC = 4,
    }

    public enum EnumStockTakeAcceptanceCertificateStatus : int
    {
        [Description("Đang chờ")]
        Waiting = 1,
        [Description("Đã duyệt")]
        Accepted = 2,
        [Description("Từ chối")]
        Rejected = 3,
    }
}
