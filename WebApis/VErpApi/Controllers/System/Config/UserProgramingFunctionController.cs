using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.ProgramingFunction;
using VErp.Services.Master.Service.ProgramingFunction;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/system/UserProgramingFunctions")]
    public class UserProgramingFunctionController : VErpBaseController
    {
        public readonly IUserProgramingFunctionService _programingFunctionForUserService;
        public UserProgramingFunctionController(IUserProgramingFunctionService programingFunctionForUserService) 
        {
            _programingFunctionForUserService = programingFunctionForUserService;
        }
        [HttpGet("List")]
        public Task<PageData<UserProgramingFunctionOutputList>> GetListFunctions([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return _programingFunctionForUserService.GetListFunctions(keyword, page, size);
        }

        [HttpPost("")]
        public Task<int> AddFunction([FromBody] UserProgramingFunctionModel model)
        {
            return _programingFunctionForUserService.AddFunction(model);
        }

        [HttpGet("{programingFunctionId}")]
        public Task<UserProgramingFunctionModel> GetFunctionInfo([FromRoute] int programingFunctionId)
        {
            return _programingFunctionForUserService.GetFunctionInfo(programingFunctionId);
        }

        [HttpPut("{programingFunctionId}")]
        public Task<bool> UpdateFunction([FromRoute] int programingFunctionId, [FromBody] UserProgramingFunctionModel model)
        {
            return _programingFunctionForUserService.UpdateFunction(programingFunctionId, model);
        }

        [HttpDelete("{programingFunctionId}")]
        public Task<bool> DeleteFunction([FromRoute] int programingFunctionId)
        {
            return _programingFunctionForUserService.DeleteFunction(programingFunctionId);
        }

        [HttpPost("ExecFunction/{programingFunctionName}")]
        public Task<IList<NonCamelCaseDictionary>> ExecSQLFunction([FromRoute] string programingFunctionName, [FromBody] NonCamelCaseDictionary<FuncParameter> inputData)
        {
            return _programingFunctionForUserService.ExecSQLFunction(programingFunctionName, inputData);
        }
    }
}
