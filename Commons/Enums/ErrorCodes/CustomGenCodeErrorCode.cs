using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{

    [ErrorCodePrefix("CGC")]
    public enum CustomGenCodeErrorCode
    {
        [Description("Không tìm thấy cấu hình sinh mã")]
        CustomConfigNotFound = 1,
        [Description("Chưa thiết định cấu hình sinh mã")]
        CustomConfigNotExisted = 2,
    }
}
