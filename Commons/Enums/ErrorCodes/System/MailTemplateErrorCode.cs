using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("MATP")]
    public enum MailTemplateErrorCode
    {
        ExistsTemplateCode = 1,
        NotFoundMailTemplate = 2,
    }
}