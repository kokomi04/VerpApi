using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.ProgramingFunction;

namespace VErp.Services.Master.Service.ProgramingFunction
{
    public interface IProgramingFunctionCustomService
    {
        Task<PageData<ProgramingFunctionCustomOutputList>> GetListFunctions(string keyword, EnumProgramingLang? programingLangId, EnumProgramingLevel? programingLevelId, int page, int size);

        Task<int> AddFunction(ProgramingFunctionCustomModel model);

        Task<ProgramingFunctionCustomModel> GetFunctionInfo(int programingFunctionId);

        Task<bool> UpdateFunction(int programingFunctionId, ProgramingFunctionCustomModel model);

        Task<bool> DeleteFunction(int programingFunctionId);

        Task<IList<NonCamelCaseDictionary>> ExecSQLFunction(string programingFunctionName, NonCamelCaseDictionary<FuncParameter> inputData);
    }
}
