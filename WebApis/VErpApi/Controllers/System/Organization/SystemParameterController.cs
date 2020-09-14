using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.SystemParameter;
using Services.Organization.Service.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/systemParameter")]
    public class SystemParameterController: VErpBaseController
    {
        private readonly ISystemParameterService _systemParameterService;

        public SystemParameterController(ISystemParameterService systemParameterService)
        {
            _systemParameterService = systemParameterService;
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<SystemParameterModel>> GetList([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _systemParameterService.GetList(keyword, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> Create([FromBody] SystemParameterModel parameterModel)
        {
            return await _systemParameterService.CreateSystemParameter(parameterModel);
        }

        [HttpPut]
        [Route("{systemParameterId}")]
        public async Task<bool> Update([FromRoute] int systemParameterId, [FromBody] SystemParameterModel parameterModel)
        {
            return await _systemParameterService.UpdateSystemParameter(systemParameterId, parameterModel);
        }

        [HttpDelete]
        [Route("{systemParameterId}")]
        public async Task<bool> Delete([FromRoute] int systemParameterId)
        {
            return await _systemParameterService.DeleteSystemParameter(systemParameterId);
        }

        [HttpGet]
        [Route("{systemParameterId}")]
        public async Task<SystemParameterModel> GetById([FromQuery] int systemParameterId)
        {
            return await _systemParameterService.GetSystemParameterById(systemParameterId);
        }
    }
}
