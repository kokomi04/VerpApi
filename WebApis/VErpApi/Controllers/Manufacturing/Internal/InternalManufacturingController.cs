using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.Step;
using VErp.Services.Manafacturing.Service.Step;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalManufacturingController: CrossServiceBaseController
    {
        private readonly IStepService _stepService;
        public InternalManufacturingController(IStepService stepService)
        {
            _stepService = stepService;
        }

        [HttpPost]
        [Route("steps/array")]
        public async Task<IList<StepModel>> GetStepByArrayId([FromBody] int[] arrayId)
        {
            return await _stepService.GetStepByArrayId(arrayId);
        }
    }
}
