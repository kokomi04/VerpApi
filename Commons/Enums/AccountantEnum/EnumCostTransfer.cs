using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public enum EnumCostTransfer : int
    {
        [Description("KC đầu 6 sang 154")]
        TYPE01 = 1,
        [Description("KC 154 sang 155 đích danh")]
        TYPE02 = 2,
        [Description("KC 154 sang 155 không đích danh")]
        TYPE03 = 3
    }
}
