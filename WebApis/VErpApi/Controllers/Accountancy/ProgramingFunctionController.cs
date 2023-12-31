﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum.Accountant;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Programing;
using VErp.Services.Accountancy.Service.Programing;

namespace VErpApi.Controllers.Accountancy
{

    [Route("api/Accountancy/programingfunctions")]

    public class ProgramingFunctionController : VErpBaseController
    {
        private readonly IProgramingFunctionService _programingFunctionService;
        public ProgramingFunctionController(IProgramingFunctionService programingFunctionService)
        {
            _programingFunctionService = programingFunctionService;
        }

        [HttpGet("List")]
        [GlobalApi]
        public Task<PageData<ProgramingFunctionOutputList>> GetListFunctions([FromQuery] string keyword, [FromQuery] EnumProgramingLang? programingLangId, [FromQuery] EnumProgramingLevel? programingLevelId, [FromQuery] int page, [FromQuery] int size)
        {
            return _programingFunctionService.GetListFunctions(keyword, programingLangId, programingLevelId, page, size);
        }

        [HttpPost("")]
        public Task<int> AddFunction([FromBody] ProgramingFunctionModel model)
        {
            return _programingFunctionService.AddFunction(model);
        }

        [HttpGet("{programingFunctionId}")]
        public Task<ProgramingFunctionModel> GetFunctionInfo([FromRoute] int programingFunctionId)
        {
            return _programingFunctionService.GetFunctionInfo(programingFunctionId);
        }

        [HttpPut("{programingFunctionId}")]
        public Task<bool> UpdateFunction([FromRoute] int programingFunctionId, [FromBody] ProgramingFunctionModel model)
        {
            return _programingFunctionService.UpdateFunction(programingFunctionId, model);
        }

        [HttpDelete("{programingFunctionId}")]
        public Task<bool> DeleteFunction([FromRoute] int programingFunctionId)
        {
            return _programingFunctionService.DeleteFunction(programingFunctionId);
        }

        [HttpPost("ExecFunction/{programingFunctionName}")]
        public Task<IList<NonCamelCaseDictionary>> ExecSQLFunction([FromRoute] string programingFunctionName, [FromBody] NonCamelCaseDictionary<FuncParameter> inputData)
        {
            return _programingFunctionService.ExecSQLFunction(programingFunctionName, inputData);
        }
    }
}