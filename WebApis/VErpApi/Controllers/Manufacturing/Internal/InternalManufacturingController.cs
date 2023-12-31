﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.Step;
using VErp.Services.Manafacturing.Service.Outsource;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Services.Manafacturing.Service.Step;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalManufacturingController : CrossServiceBaseController
    {
        private readonly IStepService _stepService;
        private readonly IProductionProcessService _productionProcessService;
        private readonly IOutsourcePartRequestService _outsourcePartRequestService;
        private readonly IOutsourceStepRequestService _outsourceStepRequestService;

        public InternalManufacturingController(
            IStepService stepService,
            IProductionProcessService productionProcessService,
            IOutsourcePartRequestService outsourcePartRequestService,
            IOutsourceStepRequestService outsourceStepRequestService)
        {
            _stepService = stepService;
            _productionProcessService = productionProcessService;
            _outsourcePartRequestService = outsourcePartRequestService;
            _outsourceStepRequestService = outsourceStepRequestService;
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
        public async Task<bool> CopyProductionProcess([FromQuery] EnumContainerType containerTypeId,[FromQuery] long fromContainerId,[FromQuery] long toContainerId)
        {
            return await _productionProcessService.CopyProductionProcess(containerTypeId, fromContainerId, toContainerId, false);
        }

        [HttpPut]
        [Route("outsourceRequest/Part/Status")]
        public async Task<bool> UpdateOutsourcePartRequestStatus([FromBody] long[] outsourcePartRequestId)
        {
            return await _outsourcePartRequestService.UpdateOutsourcePartRequestStatus(outsourcePartRequestId);
        }

        [HttpPut]
        [Route("outsourceRequest/Step/Status")]
        public async Task<bool> UpdateOutsourceStepRequestStatus([FromBody] long[] outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.UpdateOutsourceStepRequestStatus(outsourceStepRequestId);
        }
    }
}
