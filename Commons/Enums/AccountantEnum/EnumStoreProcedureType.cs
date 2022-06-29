using System.ComponentModel;

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
