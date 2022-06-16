﻿namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("USR")]
    public enum UserErrorCode
    {
        EmptyUserName = 1,
        UserNameExisted = 2,
        UserNotFound = 3,
        PasswordTooShort = 4,
        OldPasswordIncorrect = 5,
        EmployeeCodeAlreadyExisted = 6,
        GenderTypeInvalid = 7,
        StatusTypeInvalid = 8
    }
}
