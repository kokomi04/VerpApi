using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionProcessStatus
    {
        [Description("Chưa tạo")]
        NotCreatedYet = 0,
        [Description("Đã tạo")]
        Created = 1,
        [Description("Đã tạo chưa hoàn thành")]
        CreateButNotYet=2
    }
}
