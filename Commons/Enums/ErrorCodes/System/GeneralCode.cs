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
        X_ModuleMissing = 4,

        [Description("Bạn không có quyền thực hiện chức năng này")]
        Forbidden = 5,

        [Description("Không thực hiện được. Chức năng này tạm thời chưa được hỗ trợ")]
        NotYetSupported = 6,

        [Description("Đang có tranh chấp tài nguyên bởi xử lý khác, vui lòng thử lại sau ít phút")]
        DistributedLockExeption = 7,

        [Description("Item không tồn tại")]
        ItemNotFound = 8,



        [Description("Tài khoản không có quyền truy cập vào hệ thống")]
        LockedOut = 9,

        GeneralError = 10,

        [Description("Mã item đã tồn tại, vui lòng chọn mã khác!")]
        ItemCodeExisted = 11,
    }
}
