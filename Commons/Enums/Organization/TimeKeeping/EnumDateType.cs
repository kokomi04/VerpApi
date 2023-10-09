using System.ComponentModel;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumDateType
    {

        [Description("Ngày thường")]
        Weekday = 0,

        [Description("Cuối tuần")]
        Weekend = 1,

        [Description("Ngày lễ")]
        Holiday = 2,

    }
}