namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("FILE")]
    public enum FileErrorCode
    {
        FileNotFound = 1,
        InvalidFile = 2,
        InvalidFileName = 3,
        InvalidFileType = 4,
        InvalidFileExtension = 5,
        FileSizeExceededLimit = 6,
        InvalidFileStatus = 7,
        InvalidObjectType = 8,
        FileUrlExpired = 9,
        InvalidTabeInDocument = 10
    }
}
