using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumUserStatus
    {
        [Description("Chưa kích hoạt")]
        InActived = 0,
        [Description("Hoạt động")]
        Actived = 1,
        [Description("Khóa")]
        Locked = 2,
    }
}
