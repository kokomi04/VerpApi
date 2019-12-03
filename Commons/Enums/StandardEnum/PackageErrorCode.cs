using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    /// <summary>
    /// 
    /// </summary>
    public enum PackageErrorCode
    {
        [Description("Không tìm thấy kiện tương ứng")]
        PackageNotFound = 1,
        [Description("Mã kiện không có")]
        PackageCodeEmpty = 2,
        [Description("Kiện đã tồn tại")]
        PackageAlreadyExisted = 3,
        [Description("Kiện này không cho phép cập nhật")]
        PackageNotAllowUpdate = 4,
        [Description("Số lượng hàng trong kiện không đủ")]
        QualtityOfProductInPackageNotEnough = 5
    }
}
