using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    /// <summary>
    /// Tiêu chí phân bổ tính lãi lỗ
    /// </summary>
    public enum EnumCalcProfitAndLossAllocation
    {
        [Description("Giá vốn hàng bán trực tiếp")]
        PriceSellDirectly = 1,

        [Description("Chi phí bán hàng trực tiếp")]
        CostSellDirectly = 2,

        [Description("Chi phí quản lý trực tiếp")]
        CostManagerDirectly = 3,

        [Description("Tổng giá bán")]
        TotalOrderPrice = 5,

        [Description("TCPB khác")]
        OtherFee = 4
    }
}
