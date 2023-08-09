using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Programing;
using VErp.Services.Accountancy.Service.Programing;

namespace VErpApi.Controllers.Accountancy.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalProgramingFunctionController : CrossServiceBaseController
    {
        private readonly IProgramingFunctionService _programingFunctionService;
        public InternalProgramingFunctionController(IProgramingFunctionService programingFunctionService)
        {
            _programingFunctionService = programingFunctionService;
        }

        [HttpGet("Sqls")]
        public async Task<IEnumerable<ProgramingFunctionBaseModel>> Sql()
        {
            return (await _programingFunctionService.GetListFunctions("", EnumProgramingLang.Sql, null, 1, int.MaxValue)).List;
        }
    }
}