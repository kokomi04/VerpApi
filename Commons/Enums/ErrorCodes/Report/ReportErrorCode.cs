using Verp.Resources.Enums.ErrorCodes.Report;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("RTE")]

    [LocalizedDescription(ResourceType = typeof(ReportErrorCodeDescription))]
    public enum ReportErrorCode
    {
        ReportNotFound = 1,
        ReportNameAlreadyExisted = 2
    }
}
