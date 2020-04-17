﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public enum EnumOperator
    {
        [ParamNumber(1)]
        [Description("Equal")]
        Equal = 1,
        [ParamNumber(1)]
        [Description("Is Not Equal")]
        NotEqual = 2,
        [ParamNumber(1)]
        [Description("Contains")]
        Contains = 3,
        [ParamNumber(3)]
        [Description("In List")]
        InList = 4,
        [ParamNumber(0)]
        [Description("IsLeafNode")]
        IsLeafNode = 5,
    }
}
