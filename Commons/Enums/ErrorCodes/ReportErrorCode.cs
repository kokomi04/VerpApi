using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("RTE")]
    public enum ReportErrorCode
    {
        [Description("Không tìm thấy báo cáo trong hệ thống")]
        ReportNotFound = 1,
        [Description("Tên báo cáo đã tồn tại")]
        ReportNameAlreadyExisted = 2,
        CanNotGenerateReportAsDoc = 3
    }
}
