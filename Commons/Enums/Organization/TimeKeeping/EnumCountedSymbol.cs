using System.ComponentModel;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumCountedSymbol
    {
        [Description("Kí hiệu đi trễ")]
        BeLateSymbol = 1,

        [Description("Kí hiệu về sớm")]
        EarlySymbol = 2,

        [Description("Kí hiệu đúng giờ")]
        WorkOnTimeSymbol = 3,

        [Description("Kí hiệu tăng ca")]
        OvertimeSymbol = 4,

        [Description("Kí hiệu thiếu giờ ra")]
        ShortTimeoutSymbol = 5,

        [Description("Kí hiệu thiếu giờ vào")]
        ShortTimeToSymbol = 6,

        [Description("Kí hiệu vắng")]
        AbsentSymbol = 7,

        [Description("Kí hiệu đúng giờ ca có qua đêm")]

        ShiftNightSymbol = 8,

        [Description("Kí hiệu ngày không xếp ca")]
        OffSymbol = 9,
        [Description("Kí hiệu làm nửa công")]
        HalfWorkOnTimeSymbol = 10,
    }
}