using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumProductionStep
    {

    }
    public enum ProductionStepLinkDataRoleType
    {
        Input = 1,
        Output = 2,
    }

    public enum ContainerIdType
    {
        [Description("Quy trình sản xuất")]
        QTSX = 1,
        [Description("Lệnh sản xuất")]
        LSX = 2,
    }
    
}
