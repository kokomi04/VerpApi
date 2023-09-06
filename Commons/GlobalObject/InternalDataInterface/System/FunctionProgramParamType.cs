using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;

namespace VErp.Commons.GlobalObject.InternalDataInterface.System
{
    public class ProgramingFunctionBaseModel
    {
        public string ProgramingFunctionName { get; set; }
        public string FunctionBody { get; set; }
        public EnumProgramingLang ProgramingLangId { get; set; }
        public EnumProgramingLevel ProgramingLevelId { get; set; }

        public string Description { get; set; }
        public FunctionProgramParamType Params { get; set; }
    }

    public class FunctionProgramParamType
    {
        public string ReturnType { get; set; }
        public IList<FunctionProgramParamModel> ParamsList { get; set; }
    }

    public class FunctionProgramParamModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
