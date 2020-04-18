using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public enum EnumFormType
    {
        [IsRef(false)]
        Input = 1,
        [IsRef(true)]
        Select = 2,
        [IsRef(false)]
        Generate = 3,
        [IsRef(true)]
        SearchTable = 4
    }
}
