using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class UserProgramingFunction
{
    public int SubsidiaryId { get; set; }

    public int ProgramingFunctionId { get; set; }

    public string ProgramingFunctionName { get; set; }

    public string FunctionBody { get; set; }

    public string Description { get; set; }

    public string Params { get; set; }
}
