﻿using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("SP")]
    public enum StoredProcedureErrorCode
    {
        InvalidName = 1,
        InvalidExists = 2,
        InvalidType = 3,
        InvalidStartWith = 4,
        InvalidTSQl = 5
    }
}
