using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    /// <summary>
    /// Kho - Phiếu nhập xuất
    /// </summary>
    public enum EnumInventoryType
    {
        /// <summary>
        /// Nhập kho
        /// </summary>
        Input = 1,

        /// <summary>
        /// Xuất kho
        /// </summary>
        Output = 2
    }

    public enum EnumInventoryAction 
    {
        [Description("Bình thường")]
        Normal = 1,
        [Description("Xuất kho bán hàng")]
        OutputForSell = 2,
        [Description("Xuất kho sản xuất")]
        OutputForManufacture = 3,
        [Description("Nhập kho thành phẩm")]
        InputOfProduct = 4,
        [Description("Nhập kho vật tư")]
        InputOfMaterial = 5,

        [Description("Luân chuyển kho")]
        Rotation = 6
    }

    public enum EnumInventoryStatus 
    {

        Draff = 1,
        WaitToCensor = 2,
        Censored = 4,
        Reject = 6,
    }
}
