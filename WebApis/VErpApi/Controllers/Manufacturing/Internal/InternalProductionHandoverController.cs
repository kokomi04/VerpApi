using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionHandover;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductionHandoverController : CrossServiceBaseController
    {
        private readonly IProductionHandoverService _productionHandoverService;
        private readonly IMaterialAllocationService _materialAllocationService;

        public InternalProductionHandoverController(IProductionHandoverService productionHandoverService, IMaterialAllocationService materialAllocationService)
        {
            _productionHandoverService = productionHandoverService;
            _materialAllocationService = materialAllocationService;
        }

        [HttpPut]
        [Route("status")]
        public async Task<bool> ChangeAssignedProgressStatus([FromBody] ProgressStatusInputModel data)
        {
            return await _productionHandoverService.ChangeAssignedProgressStatus(data.ProductionOrderCode, data.Inventories);
        }

        [HttpPut]
        [Route("ignore-allocation")]
        public async Task<bool> UpdateIgnoreAllocation([FromBody] string[] productionOrderCodes)
        {
            return await _materialAllocationService.UpdateIgnoreAllocation(productionOrderCodes);
        }
    }
}