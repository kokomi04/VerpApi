using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("MLB")]
    public enum MediaLibraryErrorCode
    {
        [Description("Thư mục con đã tồn tại")]
        SubdirectoryExists = 1,
        [Description("Không tìm thấy thư mục")]
        NotFoundDirectory = 2,
        [Description("Trong thu mục còn chứa các file và thư mục con")]
        DirectoryNotEmpty = 3,
        GeneralError = 4
    }
}
