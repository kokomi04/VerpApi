using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VErp.Commons.Enums.Resources.Stock;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.StandardEnum
{
    /// <summary>
    /// 
    /// </summary>
    [ErrorCodePrefix("INV")]
    [LocalizedDescription(ResourceType = typeof(InventoryErrorCodeDescription))]
    public enum InventoryErrorCode
    {
        InventoryNotFound = 1,
        InventoryCodeEmpty = 2,
        InventoryAlreadyExisted = 3,
        InvalidPackage = 4,
        InventoryAlreadyApproved = 5,
        InventoryCodeAlreadyExisted = 6,
        NotEnoughQuantity = 7,
        NotSupportedYet = 8,
        InOuputAffectObjectsInvalid = 9,
        CanNotChangeStock = 10,
        InventoryNotApprovedYet = 11,
    }
}
