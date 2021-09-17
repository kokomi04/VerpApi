using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumGender
    {
        [Description("Nam")]
        [RangeValue(new[] { "Nam", "Male" })]
        Male = 1,

        [Description("Nữ")]
        [RangeValue(new[] { "Nam", "Female" })]
        Female = 2
    }
}
