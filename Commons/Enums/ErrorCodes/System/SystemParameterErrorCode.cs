﻿using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("SPE")]
    public enum SystemParameterErrorCode
    {
        SystemParameterNotFound = 1,
        SystemParameterAlreadyExisted = 2
    }
}
