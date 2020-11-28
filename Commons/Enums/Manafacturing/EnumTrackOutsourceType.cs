using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumTrackOutsourceType
    {
        OutsourceComposition = 1,
        OutsourceStages = 2
    }

    public enum EnumTrackOutsourceStatus
    {
        New = 1,
        Accept = 2,
        Processing = 3,
        Finished = 4,
        Handover = 5
    }
}
