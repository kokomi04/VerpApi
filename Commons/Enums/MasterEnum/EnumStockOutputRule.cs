using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumStockOutputRule
    {
        [Description("Không quy định")]
        None = 0,
        [Description("Vào trước ra trước")]
        Fifo = 1,
        [Description("Vào sau ra trước")]
        Lifo = 2
    }
}
