using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class ProgramingFunction
    {
        public int ProgramingFunctionId { get; set; }
        public string ProgramingFunctionName { get; set; }
        public string FunctionBody { get; set; }
        public int ProgramingLangId { get; set; }
        public int ProgramingLevelId { get; set; }
        public string Description { get; set; }
        public string Params { get; set; }
    }
}
