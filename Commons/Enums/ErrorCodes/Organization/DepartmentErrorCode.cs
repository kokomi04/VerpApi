using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("DEPT")]
    public enum DepartmentErrorCode
    {
        [Description("Không tìm thấy bộ phận")]
        DepartmentNotFound = 1,
        [Description("Không tìm thấy bộ phận chủ quản")]
        DepartmentParentNotFound = 2,
        [Description("Mã đối bộ phận đã tồn tại")]
        DepartmentCodeAlreadyExisted = 3,
        [Description("Tên bộ phận đã tồn tại")]
        DepartmentNameAlreadyExisted = 4,
        [Description("Tồn tại bộ phận trực thuộc")]
        DepartmentChildAlreadyExisted = 5,
        [Description("Tồn tại bộ phận trực thuộc đang hoạt động")]
        DepartmentChildActivedAlreadyExisted = 6,
        [Description("Tồn tại nhân sự trực thuộc đang hoạt động")]
        DepartmentUserActivedAlreadyExisted = 7,
        [Description("Bộ phận đang hoạt động, chỉ được phép xóa bộ phận không hoạt động")]
        DepartmentActived = 8,
        [Description("Bộ phận chủ quản đang không hoạt động")]
        DepartmentParentInActived = 9,
        [Description("Bộ phận đang không hoạt động")]
        DepartmentInActived = 10,
    }
}
