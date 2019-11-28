using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.StandardEnum
{
    /// <summary>
    /// 
    /// </summary>
    public enum InventoryErrorCode
    {
        InventoryNotFound = 1,
        InventoryCodeEmpty = 2,
        InventoryAlreadyExisted = 3
    }

    public enum InventoryDetailErrorCode
    {
        InventoryDetailNotFound = 1,
        InventoryDetailCodeEmpty = 2,
        InventoryDetailAlreadyExisted = 3,
        OutOfStock = 4,
    }
}
