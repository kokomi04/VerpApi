﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public static class EnumProductionProcess
    {
        public enum ContainerType
        {
            [Description("Sản phẩm")]
            SP = 1,
            [Description("Lệnh sản xuất")]
            LSX = 2,
        }
        public enum ProductionStepLinkDataRoleType
        {
            Input = 1,
            Output = 2,
        }
    }
    

    
    
}
