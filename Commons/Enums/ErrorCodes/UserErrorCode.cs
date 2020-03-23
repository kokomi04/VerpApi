using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("USR")]
    public enum UserErrorCode
    {
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        EmptyUserName = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        UserNameExisted = 2,
        [EnumStatusCode(HttpStatusCode.NotFound)]
        UserNotFound = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        PasswordTooShort = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        OldPasswordIncorrect = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        EmployeeCodeAlreadyExisted = 6
    }
}
