using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("GEN")]
    public enum GeneralCode
    {
        [Description("Thành công")]
        Success = 0,

        [Description("Tham số không hợp lệ")]
        InvalidParams = 2,

        [Description("Lỗi hệ thống")]
        InternalError = 3,

        [Description("X-Module header was not found")]
        X_ModuleMissing = 3,

        [Description("Bạn không có quyền thực hiện chức năng này")]
        Forbidden = 4
    }
}
