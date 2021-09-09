using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Verp.Resources.Enums.ErrorCodes.Manufacturing;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.ErrorCodes
{
    [ErrorCodePrefix("OUTS")]
    [LocalizedDescription(ResourceType = typeof(OutsourceErrorCodeDescription))]
    public enum OutsourceErrorCode
    {
        NotFoundRequest = 1,
        NotFoundOutsourceOrder = 2,
        InValidRequestOutsource = 3,
        OutsoureOrderCodeAlreadyExisted = 4,
        NotFoundOutsourOrderDetail = 5,
        EarlyExistsProductionStepHadOutsourceRequest = 6,
        CanNotCreateOutsourceOrder = 7,
        HasPurchaseOrder = 8,
    }
}
