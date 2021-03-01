using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Commons.Enums.Manafacturing;
using VErp.Services.Manafacturing.Service.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/[controller]")]
    [ApiController]
    public class ProductionScheduleTurnShiftController : VErpBaseController
    {
        private readonly IProductionScheduleTurnShiftService _productionScheduleTurnShiftService;

        public ProductionScheduleTurnShiftController(IProductionScheduleTurnShiftService productionScheduleTurnShiftService)
        {
            _productionScheduleTurnShiftService = productionScheduleTurnShiftService;
        }


        [HttpGet]
        [Route("")]
        public async Task<IList<ProductionScheduleTurnShiftModel>> GetShifts([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId)
        {
            return await _productionScheduleTurnShiftService.GetShifts(departmentId, productionOrderId, productionStepId);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateShift([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId
            , [FromBody] ProductionScheduleTurnShiftModel model)
        {
            return await _productionScheduleTurnShiftService.CreateShift(departmentId, productionOrderId, productionStepId, model);
        }

        [HttpPut]
        [Route("{productionScheduleTurnShiftId}")]
        public async Task<bool> UpdateShift([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId
          , [FromRoute] long productionScheduleTurnShiftId
          , [FromBody] ProductionScheduleTurnShiftModel model)
        {
            return await _productionScheduleTurnShiftService.UpdateShift(departmentId, productionOrderId, productionStepId, productionScheduleTurnShiftId, model);
        }

        [HttpDelete]
        [Route("{productionScheduleTurnShiftId}")]
        public async Task<bool> DeleteShift([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId
        , [FromRoute] long productionScheduleTurnShiftId)
        {
            return await _productionScheduleTurnShiftService.DeleteShift(departmentId, productionOrderId, productionStepId, productionScheduleTurnShiftId);
        }

    }
}
