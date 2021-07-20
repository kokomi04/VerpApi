using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;
using VErp.Commons.Enums.Resources.ErrorCodes.PO;

namespace VErp.Commons.Enums.ErrorCodes
{
    [LocalizedDescription(ResourceType = typeof(PurchasingRequestErrorCodeDescription))]
    public enum PurchasingRequestErrorCode
    {
        RequestNotFound = 1,
        RequestCodeEmpty = 2,
        RequestCodeAlreadyExisted = 3,
      
        //RequestAlreadyApproved = 4,
    }
}
