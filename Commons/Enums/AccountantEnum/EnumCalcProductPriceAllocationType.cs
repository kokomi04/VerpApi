using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    /// <summary>
    /// Tiêu chí phân bổ tính giá thành
    /// </summary>
    public enum EnumCalcProductPriceAllocationType
    {
        /// <summary>
        /// TCPB Nguyên vật liệu trực tiếp
        /// </summary>
        DirectMaterialFee = 1,
        /// <summary>
        /// TCPB Nhân công trực tiếp
        /// </summary>
        DirectLaborFee = 2,

        /// <summary>
        /// TCPB Tổng giá bán
        /// </summary>
        TotalSellPrice = 3,
    }
}
