using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Organization;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes.Organization
{
    [ErrorCodePrefix("Subsidiary")]
    [LocalizedDescription(ResourceType = typeof(SubsidiaryErrorCodeDescription))]
    public enum SubsidiaryErrorCode
    {
        SubsidiaryNotfound = 1,
        SubsidiaryCodeExisted = 2,
        SubsidiaryNameExisted = 3,
    }
}
