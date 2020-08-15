using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    /// <summary>
    /// Tiêu chí phân bổ tính giá thành
    /// </summary>
    public enum EnumCalcProductPriceAllocationCriteria
    {
        /// <summary>
        /// TCPB Nguyên vật liệu trực tiếp
        /// </summary>
        TCPB_NVL_TT = 1,
        /// <summary>
        /// TCPB Nhân công trực tiếp
        /// </summary>
        TCPB_NHANC_TT = 2,

        /// <summary>
        /// TCPB Tổng giá bán
        /// </summary>
        TCPB_TONG_GIABAN = 3,
    }
}
