using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Programing;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Service.Programing
{
    public interface IProgramingFunctionService
    {
        Task<PageData<ProgramingFunctionOutputList>> GetListFunctions(string keyword, EnumProgramingLang? programingLangId, EnumProgramingLevel? programingLevelId, int page, int size);

        Task<int> AddFunction(ProgramingFunctionModel model);

        Task<ProgramingFunctionModel> GetFunctionInfo(int programingFunctionId);

        Task<bool> UpdateFunction(int programingFunctionId, ProgramingFunctionModel model);

        Task<bool> DeleteFunction(int programingFunctionId);

        Task<IList<NonCamelCaseDictionary>> ExecSQLFunction(string programingFunctionName, NonCamelCaseDictionary<FuncParameter> inputData);
    }
}
