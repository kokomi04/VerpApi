using System.ComponentModel;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumArrangeShiftMode
    {
        
        [Description("Sắp xếp theo tuần")]
        ArrangeByWeek = 1,
        
        [Description("Sắp xếp theo tháng")]
        ArrangeByMonth = 2,
        
        [Description("Sắp xếp theo năm")]
        ArrangeByYear = 3,
    }
}