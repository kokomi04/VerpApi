﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Dictionary
{
    public class UnitInput
    {
        public string UnitName { get; set; }
        public EnumUnitStatus UnitStatusId { get; set; }
    }
}