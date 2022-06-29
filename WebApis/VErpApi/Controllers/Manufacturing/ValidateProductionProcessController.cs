﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidateProductionProcessController : VErpBaseController
    {
        private readonly IValidateProductionProcessService _validateProductionProcessService;
        private readonly IProductionProcessService _productionProcessService;

        public ValidateProductionProcessController(IValidateProductionProcessService validateProductionProcessService, IProductionProcessService productionProcessService)
        {
            _validateProductionProcessService = validateProductionProcessService;
            _productionProcessService = productionProcessService;
        }

        [HttpPost]
        [Route("warning/client")]
        public async Task<IList<ProductionProcessWarningMessage>> ValidateProductionProcessClient([FromQuery] EnumContainerType containerTypeId, [FromQuery] long containerId, [FromBody] ProductionProcessModel productionProcess)
        {
            return await _validateProductionProcessService.ValidateProductionProcess(containerTypeId, containerId, productionProcess);
        }

        [HttpPost]
        [Route("warning")]
        public async Task<IEnumerable<ProductionProcessWarningMessage>> ValidateProductionProcess([FromQuery] EnumContainerType containerTypeId, [FromQuery] long containerId)
        {
            var productionProcess = await _productionProcessService.GetProductionProcessByContainerId(containerTypeId, containerId);

            return await _validateProductionProcessService.ValidateProductionProcess(containerTypeId, containerId, productionProcess);
        }

        [HttpGet]
        [Route("validateOutsourceStepRequest")]
        public async Task<bool> ValidateOutsourceStepRequest([FromQuery] long containerId, [FromQuery] long outsourceStepRequestId)
        {
            var productionProcess = await _productionProcessService.GetProductionProcessByContainerId(EnumContainerType.ProductionOrder, containerId);
            var lsWarningMessage = await _validateProductionProcessService.ValidateOutsourceStepRequest(productionProcess);

            return !lsWarningMessage.Any(x => x.ObjectId == outsourceStepRequestId);
        }

        [HttpGet]
        [Route("validateOutsourcePartRequest")]
        public async Task<bool> ValidateOutsourcePartRequest([FromQuery] long containerId, [FromQuery] long outsourcePartRequestId)
        {
            var productionProcess = await _productionProcessService.GetProductionProcessByContainerId(EnumContainerType.ProductionOrder, containerId);
            var lsWarningMessage = await _validateProductionProcessService.ValidateOutsourcePartRequest(productionProcess);

            return !lsWarningMessage.Any(x => x.ObjectId == outsourcePartRequestId);
        }
    }
}
