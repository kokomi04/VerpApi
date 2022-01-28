using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Organization;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("ROL")]
    [LocalizedDescription(ResourceType = typeof(RoleErrorCodeDescription))]

    public enum RoleErrorCode
    {
        EmptyRoleName = 1,
        RoleNotFound = 3,
        RoleIsReadonly = 4,
        LoopbackParentRole = 5,
        ParentRoleNotFound = 6,
        ExistedChildrenRoles = 7,
        ExistsUserInRole = 8,
    }
}
