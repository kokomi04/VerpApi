using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Manufacturing;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    [ErrorCodePrefix("PROD")]
    [LocalizedDescription(ResourceType = typeof(ProductOrderErrorCodeDescription))]
    public enum ProductOrderErrorCode
    {
        ProductOrderNotfound = 1,
        ProductOrderCodeAlreadyExisted = 2,
        NotFoundMaterials = 3
    }
}
