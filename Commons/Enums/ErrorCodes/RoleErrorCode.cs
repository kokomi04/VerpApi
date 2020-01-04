using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("ROL")]
    public enum RoleErrorCode
    {
        EmptyRoleName = 1,
        RoleNotFound = 3,
        RoleIsReadonly = 4
    }
}
