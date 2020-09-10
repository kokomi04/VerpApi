using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public enum EnumStoreProcedureType
    {
        [Description("VIEW")]
        View = 1,
        [Description("PROCEDURE")]
        Procedure = 2,
        [Description("FUNCTION")]
        Function = 3,
    }
}
