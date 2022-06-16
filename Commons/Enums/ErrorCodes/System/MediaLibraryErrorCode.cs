using Verp.Resources.Enums.System;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("MLB")]
    [LocalizedDescription(ResourceType = typeof(MediaLibraryErrorCodeDescription))]

    public enum MediaLibraryErrorCode
    {
        SubdirectoryExists = 1,
        NotFoundDirectory = 2,
        DirectoryNotEmpty = 3,
    }
}
