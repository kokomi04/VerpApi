using Verp.Resources.Enums.System;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("I18N")]
    [LocalizedDescription(ResourceType = typeof(I18nLanguageErrorCodeDescription))]

    public enum I18nLanguageErrorCode
    {
        ItemNotFound,
        AlreadyExistsKeyCode
    }
}