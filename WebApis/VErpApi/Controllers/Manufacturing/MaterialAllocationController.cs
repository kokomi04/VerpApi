using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialAllocationController : VErpBaseController
    {
        private readonly IMaterialAllocationService _materialAllocationService;

        public MaterialAllocationController(IMaterialAllocationService materialAllocationService)
        {
            _materialAllocationService = materialAllocationService;
        }

        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<IList<MaterialAllocationModel>> GetMaterialAllocations([FromRoute] long productionOrderId)
        {
            return await _materialAllocationService.GetMaterialAllocations(productionOrderId);
        }

        [HttpPut]
        [Route("{productionOrderId}")]
        public async Task<AllocationModel> UpdateMaterialAllocation([FromRoute] long productionOrderId, [FromBody] AllocationModel data)
        {
            return await _materialAllocationService.UpdateMaterialAllocation(productionOrderId, data);
        }

        [HttpGet]
        [Route("{productionOrderId}/ignore")]
        public async Task<IList<IgnoreAllocationModel>> GetIgnoreAllocations([FromRoute] long productionOrderId)
        {
            return await _materialAllocationService.GetIgnoreAllocations(productionOrderId);
        }

        [HttpGet]
        [Route("{productionOrderId}/conflict")]
        public async Task<ConflictHandoverModel> GetConflictHandovers([FromRoute] long productionOrderId)
        {
            return await _materialAllocationService.GetConflictHandovers(productionOrderId);
        }
    }
}
