using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("ACCT")]
    public enum AccountingAccountErrorCode
    {
        [Description("Không tìm thấy tài khoản")]
        AccountingAccountNotFound = 1,
        [Description("Số tài khoản đã tồn tại")]
        AccountingAccountNumberAlreadyExisted = 2,
        [Description("Tên tài khoản tiếng Việt đã tồn tại")]
        AccountNameViAlreadyExisted = 3,
        [Description("Tên tài khoản tiếng Anh đã tồn tại")]
        AccountNameEnAlreadyExisted = 4,
        [Description("Không tìm thấy tài khoản chủ quản")]
        AccountingAccountParentNotFound = 5,
        [Description("Tồn tại tài khoản trực thuộc")]
        AccountingAccountChildNotFound = 6,
    }
}
