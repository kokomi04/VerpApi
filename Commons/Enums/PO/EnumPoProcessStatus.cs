﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Resources.PO;
using VErp.Commons.ObjectExtensions.CustomAttributes;

namespace VErp.Commons.Enums.MasterEnum.PO
{
    [LocalizedDescription(typeof(EnumPoProcessStatusDescription))]
    public enum EnumPoProcessStatus
    {
        Normal = 0,
        SentToProvider = 1,
        Completed = 2
    }
}