using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumObjectType
    {
        UserAndEmployee = 1,
        Role = 2,
        RolePermission = 3,
        ProductCate = 4,
        ProductType = 5,
        Product = 6,
        Unit = 7,
        BarcodeConfig = 8,
        /// <summary>
        /// Kho
        /// </summary>
        Stock = 9, 
        File = 10,
        /// <summary>
        /// Vị trí trong Kho         
        /// </summary>
        Location = 11, 
    }
}
