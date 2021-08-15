using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

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
        [Description("Hoàn thành")]
        Finish = 3,
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
