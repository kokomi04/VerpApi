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
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionOrder;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductionScheduleController : CrossServiceBaseController
    {
        private readonly IProductionScheduleService _productionScheduleService;
        public InternalProductionScheduleController(IProductionScheduleService productionScheduleService)
        {
            _productionScheduleService = productionScheduleService;
        }

        [HttpPut]
        [Route("{scheduleTurnId}/status/{status}")]
        public async Task<bool> UpdateProductionScheduleStatus([FromRoute] long scheduleTurnId, [FromBody] ProductionScheduleStatusModel status)
        {
            return await _productionScheduleService.UpdateProductionScheduleStatus(scheduleTurnId, status);
        }
    }
}