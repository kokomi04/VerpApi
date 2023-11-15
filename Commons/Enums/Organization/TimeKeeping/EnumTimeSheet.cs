using System.ComponentModel;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumTimeSheetDateType
    {

        [Description("Ngày thường")]
        Weekday = 0,

        [Description("Cuối tuần")]
        Weekend = 1,

        [Description("Ngày lễ")]
        Holiday = 2,

    }

    public enum EnumTimeSheetOvertimeType
    {
        [Description("Tăng ca tổng hợp")]
        Default = 0,

        [Description("Tăng ca trước giờ làm việc")]
        BeforeWork = 1,

        [Description("Tăng ca sau giờ làm việc")]
        AfterWork = 2,

        [Description("Ngày tăng ca")]
        DateAsOvertime = 3,
    }
    public enum EnumTimeSheetMode
    {
        [Description("Theo ca")]
        ByShift = 0,

        [Description("Theo giờ vào ra")]
        ByCheckinCheckoutInDay = 1,

        [Description("Làm công nhật")]
        ByDayLabour = 2,

        [Description("Vắng")]
        Absence = 3,
    }

}