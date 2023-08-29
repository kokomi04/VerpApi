using System.ComponentModel;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumPartialShiftCalculationMode
    {
        [Description("Tính theo số giờ làm thực tế")]
        CalculateByHours = 0,

        [Description("Tính nửa công")]
        CalculateByHalfDay = 1
    }
}