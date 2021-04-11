using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountancyDB;

namespace VErp.Services.Accountancy.Model.Programing
{
    public class ProgramingFunctionModel : IMapFrom<ProgramingFunction>
    {
        public string ProgramingFunctionName { get; set; }
        public string FunctionBody { get; set; }
        public int ProgramingLangId { get; set; }
        public int ProgramingLevelId { get; set; }

        public string Description { get; set; }
        public string Params { get; set; }
    }
    public class ProgramingFunctionOutputList : ProgramingFunctionModel
    {
        public int ProgramingFunctionId { get; set; }
    }

    public class FuncParameter
    {
        public EnumDataType DataType { get; set; }
        public object Value { get; set; }
    }
}
