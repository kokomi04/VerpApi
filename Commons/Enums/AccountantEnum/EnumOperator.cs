using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public enum EnumOperator
    {
        [Description("Equal")]
        Equal = 1,
        [Description("Is Not Equal")]
        NotEqual = 2,
        [Description("Contains")]
        Contains = 3,
        [Description("In List")]
        InList = 4
    }
}
