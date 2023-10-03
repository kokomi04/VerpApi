using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.Enums.Organization.TimeKeeping
{
    public enum TimeKeepingMethodType
    {
        [Description("Chấm công qua phần mềm")]
        Software = 0,

        [Description("Chấm bằng máy chấm công")]
        Machine = 1
    }
}
