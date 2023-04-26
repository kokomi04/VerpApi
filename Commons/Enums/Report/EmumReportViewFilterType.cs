using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Report
{
    public enum EmumReportViewFilterType
    {
        [Description("Bộ lọc người dùng chọn")]
        Filter = 0,

        [Description("Người dùng thiết lập")]
        Setting = 1
    }
}
