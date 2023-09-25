using System.ComponentModel;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumShiftScheduleMode
    {

        [Description("Phân ca theo tuần")]
        ByWeek = 0,

        [Description("Phân ca theo ngày")]
        ByDay = 1,
    }
}