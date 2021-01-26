using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Step;
using VErp.Services.Manafacturing.Service.Step;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/steps")]
    [ApiController]
    public class StepController : VErpBaseController
    {
        private readonly IStepService _stepService;

        public StepController(IStepService stepService)
        {
            _stepService = stepService;
        }

        [HttpPost]
        [Route("")]
        public async Task<int> CreateStep([FromBody] StepModel req)
        {
            return await _stepService.CreateStep(req);
        }

        [HttpPut]
        [Route("{stepId}")]
        public async Task<bool> UpdateStep([FromRoute] int stepId, [FromBody] StepModel req)
        {
            return await _stepService.UpdateStep(stepId, req);
        }

        [HttpDelete]
        [Route("{stepId}")]
        public async Task<bool> DeleteStep([FromRoute] int stepId)
        {
            return await _stepService.DeleteStep(stepId);
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<StepModel>> GetListStep([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _stepService.GetListStep(keyword, page, size);
        }
    }
}
