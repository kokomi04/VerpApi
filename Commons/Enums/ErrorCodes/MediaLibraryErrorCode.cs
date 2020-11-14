using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("MLB")]
    public enum MediaLibraryErrorCode
    {
        SubdirectoryExists = 1,
        NotFoundDirectory = 2,
        DirectoryNotEmpty = 3,
        GeneralError = 4
    }
}
