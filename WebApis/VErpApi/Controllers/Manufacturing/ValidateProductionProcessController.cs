using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IList<ProductionProcessWarningMessage>> ValidateProductionProcessClient([FromQuery]EnumContainerType containerTypeId, [FromQuery] long containerId,[FromBody] ProductionProcessModel productionProcess)
        {
            return await _validateProductionProcessService.ValidateProductionProcess(containerTypeId, containerId, productionProcess);
        }

        [HttpPost]
        [Route("warning")]
        public async Task<IList<ProductionProcessWarningMessage>> ValidateProductionProcess([FromQuery] EnumContainerType containerTypeId, [FromQuery] long containerId)
        {
            var productionProcess = await _productionProcessService.GetProductionProcessByContainerId(containerTypeId, containerId);
            return await _validateProductionProcessService.ValidateProductionProcess(containerTypeId, containerId, productionProcess);
        }
    }
}
