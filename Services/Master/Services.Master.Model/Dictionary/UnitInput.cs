﻿using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.Dictionary
{
    public class UnitInput
    {
        public string UnitName { get; set; }
        public EnumUnitStatus UnitStatusId { get; set; }
        public int DecimalPlace { get; set; }
    }
}
