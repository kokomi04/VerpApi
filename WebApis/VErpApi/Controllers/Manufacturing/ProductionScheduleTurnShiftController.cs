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
        [Route("Departments/{departmentId}/scheduleTurns/{scheduleTurnId}/steps/{productionStepId}/shifts")]
        public async Task<IList<ProductionScheduleTurnShiftModel>> GetShifts([FromRoute] int departmentId, [FromRoute] long scheduleTurnId, [FromRoute] long productionStepId)
        {
            return await _productionScheduleTurnShiftService.GetShifts(departmentId, scheduleTurnId, productionStepId);
        }

        [HttpPost]
        [Route("Departments/{departmentId}/scheduleTurns/{scheduleTurnId}/steps/{productionStepId}/shifts")]
        public async Task<long> CreateShift([FromRoute] int departmentId, [FromRoute] long scheduleTurnId, [FromRoute] long productionStepId
            , [FromBody] ProductionScheduleTurnShiftModel model)
        {
            return await _productionScheduleTurnShiftService.CreateShift(departmentId, scheduleTurnId, productionStepId, model);
        }

        [HttpPut]
        [Route("Departments/{departmentId}/scheduleTurns/{scheduleTurnId}/steps/{productionStepId}/shifts/{productionScheduleTurnShiftId}")]
        public async Task<bool> UpdateShift([FromRoute] int departmentId, [FromRoute] long scheduleTurnId, [FromRoute] long productionStepId
          , [FromRoute] long productionScheduleTurnShiftId
          , [FromBody] ProductionScheduleTurnShiftModel model)
        {
            return await _productionScheduleTurnShiftService.UpdateShift(departmentId, scheduleTurnId, productionStepId, productionScheduleTurnShiftId, model);
        }

        [HttpDelete]
        [Route("Departments/{departmentId}/scheduleTurns/{scheduleTurnId}/steps/{productionStepId}/shifts/{productionScheduleTurnShiftId}")]
        public async Task<bool> DeleteShift([FromRoute] int departmentId, [FromRoute] long scheduleTurnId, [FromRoute] long productionStepId
        , [FromRoute] long productionScheduleTurnShiftId)
        {
            return await _productionScheduleTurnShiftService.DeleteShift(departmentId, scheduleTurnId, productionStepId, productionScheduleTurnShiftId);
        }

    }
}
