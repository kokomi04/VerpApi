using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionProcess;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialAllocationController : VErpBaseController
    {
        private readonly IMaterialAllocationService _materialAllocationService;
        private readonly IProductionProgressService _productionProgressService;

        public MaterialAllocationController(IMaterialAllocationService materialAllocationService, IProductionProgressService productionProgressService)
        {
            _materialAllocationService = materialAllocationService;
            _productionProgressService = productionProgressService;
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


        [HttpGet]
        [Route("{productionOrderId}/ConflictInventories")]
        public async Task<IList<ProductionOrderInventoryConflictModel>> GetConflictInventories([FromRoute] long productionOrderId)
        {
            return await _productionProgressService.GetConflictInventories(productionOrderId);
        }
    }
}
