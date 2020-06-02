using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class ProgramingFunction
    {
        public int ProgramingFunctionId { get; set; }
        public string ProgramingFunctionName { get; set; }
        public string FunctionBody { get; set; }
        public int ProgramingLangId { get; set; }
        public int ProgramingLevelId { get; set; }
    }
}
