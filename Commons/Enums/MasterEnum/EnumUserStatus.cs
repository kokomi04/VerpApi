﻿using System.ComponentModel;

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
