﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Step;
using VErp.Services.Manafacturing.Service.Step;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/stepGroups")]
    [ApiController]
    public class StepGroupController : VErpBaseController
    {
        private readonly IStepGroupService _stepGroupService;

        public StepGroupController(IStepGroupService stepGroupService)
        {
            _stepGroupService = stepGroupService;
        }

        [HttpPost]
        [Route("")]
        public async Task<int> CreateStepGroup([FromBody] StepGroupModel req)
        {
            return await _stepGroupService.CreateStepGroup(req);
        }

        [HttpPut]
        [Route("{stepGroupId}")]
        public async Task<bool> UpdateStepGroup([FromRoute] int stepGroupId, [FromBody] StepGroupModel req)
        {
            return await _stepGroupService.UpdateStepGroup(stepGroupId, req);
        }

        [HttpDelete]
        [Route("{stepGroupId}")]
        public async Task<bool> DeleteStepGroup([FromRoute] int stepGroupId)
        {
            return await _stepGroupService.DeleteStepGroup(stepGroupId);
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<StepGroupModel>> GetListStepGroup([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stepGroupService.GetListStepGroup(keyword, page, size);
        }
    }
}
