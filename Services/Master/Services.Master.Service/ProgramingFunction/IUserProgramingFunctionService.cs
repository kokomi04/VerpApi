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
    public interface IUserProgramingFunctionService
    {
        Task<PageData<UserProgramingFunctionOutputList>> GetListFunctions(string keyword, int page, int size);

        Task<int> AddFunction(UserProgramingFunctionModel model);

        Task<UserProgramingFunctionModel> GetFunctionInfo(int programingFunctionId);

        Task<bool> UpdateFunction(int programingFunctionId, UserProgramingFunctionModel model);

        Task<bool> DeleteFunction(int programingFunctionId);

        Task<IList<NonCamelCaseDictionary>> ExecSQLFunction(string programingFunctionName, NonCamelCaseDictionary<FuncParameter> inputData);
    }
}
