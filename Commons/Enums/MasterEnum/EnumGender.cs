using System.ComponentModel;

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
