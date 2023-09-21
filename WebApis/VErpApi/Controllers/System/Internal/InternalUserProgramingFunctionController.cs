using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Service.ProgramingFunction;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalUserProgramingFunctionController : CrossServiceBaseController
    {
        private readonly IUserProgramingFunctionService _userProgramingFunctionService;
        public InternalUserProgramingFunctionController(IUserProgramingFunctionService userProgramingFunctionService)
        {
            _userProgramingFunctionService = userProgramingFunctionService;
        }

        [HttpGet("")]
        public async Task<IEnumerable<UserProgramingFuctionModel>> GetListFunction()
        {
            return (await _userProgramingFunctionService.GetListFunctions("", 1, int.MaxValue)).List;
        }

    }
}
