using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum EnumOvertimeMode
    {
        [Description("Theo thời gian vào ra")]
        ByWorkingTime = 0,

        [Description("Theo đơn tăng ca")]
        ByOvertimeOrder = 1,

        [Description("Theo cả hai")]
        ByAll = 2,
    }
    public enum EnumOvertimeCalculationMode
    {
        [Description("Tính theo số giờ làm thực tế")]
        ByActualWorkingHours = 0,

        [Description("Tính theo tổng số giờ đến sớm và về muộn")]
        ByTotalEarlyLateHours = 1
    }
}
