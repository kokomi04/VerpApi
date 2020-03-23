using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("ROL")]
    public enum RoleErrorCode
    {
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Tên nhóm quyền không được để trống")]
        EmptyRoleName = 1,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        [Description("Nhóm quyền không tồn tại")]
        RoleNotFound = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Nhóm quyền không được phép thay đổi")]
        RoleIsReadonly = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Nhóm cha không được là chính nó")]
        LoopbackParentRole = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Nhóm cha không tồn tại")]
        ParentRoleNotFound = 6,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        [Description("Đang tồn tại nhóm con")]
        ExistedChildrenRoles = 7,
    }
}
