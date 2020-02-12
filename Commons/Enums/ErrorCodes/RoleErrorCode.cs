using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("ROL")]
    public enum RoleErrorCode
    {
        [Description("Tên nhóm quyền không được để trống")]
        EmptyRoleName = 1,
        [Description("Nhóm quyền không tồn tại")]
        RoleNotFound = 3,
        [Description("Nhóm quyền không được phép thay đổi")]
        RoleIsReadonly = 4,
        [Description("Nhóm cha không được là chính nó")]
        LoopbackParentRole = 5,
        [Description("Nhóm cha không tồn tại")]
        ParentRoleNotFound = 6,
    }
}
