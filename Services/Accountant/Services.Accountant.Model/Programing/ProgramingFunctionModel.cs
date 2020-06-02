using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Programing
{
    public class ProgramingFunctionModel : IMapFrom<ProgramingFunction>
    {
        public string ProgramingFunctionName { get; set; }
        public string FunctionBody { get; set; }
        public int ProgramingLangId { get; set; }
        public int ProgramingLevelId { get; set; }
    }
    public class ProgramingFunctionOutputList : ProgramingFunctionModel
    {
        public int ProgramingFunctionId { get; set; }
    }
}
