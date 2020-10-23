using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("VSD")]
    public enum VisualDirectoryErrorCode
    {
        SubdirectoryExists = 1,
        NotFoundVisualDirectory = 2
    }
}
