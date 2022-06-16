using Verp.Resources.Enums.System;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{

    [ErrorCodePrefix("CGC")]
    [LocalizedDescription(ResourceType = typeof(CustomGenCodeErrorCodeDescription))]

    public enum CustomGenCodeErrorCode
    {
        CustomConfigNotFound = 1
    }
}
