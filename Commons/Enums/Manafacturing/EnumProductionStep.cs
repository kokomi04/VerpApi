using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public enum EnumInOutStepType
    {
        InputStep = 1,
        OutputStep = 2,
    }

    public enum EnumUsingType
    {
        [Description("Quy trình sản xuất")]
        QTSX = 1,
        [Description("Lệnh sản xuất")]
        LSX = 2,
    }

    public enum ProductTypeInStages
    {
        Product = 1,
        SemiProduct = 2,
    }
}
