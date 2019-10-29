using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("USR")]
    public enum UserErrorCode
    {
        EmptyUserName = 1,
        UserNameExisted = 2,
        UserNotFound = 3,
        PasswordTooShort = 4,
        OldPasswordIncorrect = 5
    }
}
