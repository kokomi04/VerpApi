using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Organization.Salary
{
    public enum EnumSalaryPeriodCensorStatus
    {
        [Description("Mới")]
        New = 0,
        [Description("Kiểm tra thành công")]
        CheckedAccepted = 1,
        [Description("Kiểm tra từ chối")]
        CheckedRejected = 2,
        [Description("Duyệt thành công")]
        CensorApproved = 3,
        [Description("Duyệt từ chối")]
        CensorRejected = 4
    }
}
