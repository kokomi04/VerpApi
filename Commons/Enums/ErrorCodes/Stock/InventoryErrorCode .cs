﻿using Verp.Resources.Enums.Stock;
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
        CanNotChangeProductInventoryHasRequirement = 12,
        InventoryNotSentToCensorYet = 13,
        InventoryAlreadyRejected = 14,
        InventoryNotPassCheckYet = 15,
        InventoryNotDraffYet = 16,
        InventoryNotCensoredYet = 17,
    }
}
