using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.MasterEnum
{
    public enum EnumQuantitativeUnitType
    {
        [Description("g/m2")]
        GamOverAcreageM2 = 1,
        [Description("g/m3")]
        GamOverVolumeM3 = 2
    }
}
