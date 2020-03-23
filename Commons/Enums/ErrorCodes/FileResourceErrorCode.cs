using System.Net;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("FILE")]
    public enum FileErrorCode
    {
        [EnumStatusCode(HttpStatusCode.NotFound)]
        FileNotFound = 1,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        InvalidFile = 2,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        InvalidFileName = 3,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        InvalidFileType = 4,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        InvalidFileExtension = 5,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        FileSizeExceededLimit = 6,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        InvalidFileStatus = 7,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        InvalidObjectType = 8,
        [EnumStatusCode(HttpStatusCode.BadRequest)]
        FileUrlExpired = 9
    }
}
