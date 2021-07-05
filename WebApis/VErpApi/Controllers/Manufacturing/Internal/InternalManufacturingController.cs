using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.Step;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Services.Manafacturing.Service.Step;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalManufacturingController: CrossServiceBaseController
    {
        private readonly IStepService _stepService;
        private readonly IProductionProcessService _productionProcessService;

        public InternalManufacturingController(IStepService stepService, IProductionProcessService productionProcessService)
        {
            _stepService = stepService;
            _productionProcessService = productionProcessService;
        }

        [HttpPost]
        [Route("steps/array")]
        public async Task<IList<StepModel>> GetStepByArrayId([FromBody] int[] arrayId)
        {
            return await _stepService.GetStepByArrayId(arrayId);
        }

        [HttpGet]
        [Route("steps")]
        public async Task<IList<StepModel>> GetSteps()
        {
            return (await _stepService.GetListStep("", -1, -1)).List;
        }

        [HttpPost]
        [Route("productionProcess/copy")]
        public async Task<bool> CopyProductionProcess(EnumContainerType containerTypeId, long fromContainerId, long toContainerId)
        {
            return await _productionProcessService.CopyProductionProcess(containerTypeId, fromContainerId, toContainerId);
        }
    }
}
