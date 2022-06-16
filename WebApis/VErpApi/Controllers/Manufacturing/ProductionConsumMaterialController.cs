using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Services.Manafacturing.Service.ProductionAssignment;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/[controller]")]
    [ApiController]
    public class ProductionConsumMaterialController : VErpBaseController
    {
        private readonly IProductionConsumMaterialService _productionConsumMaterialService;

        public ProductionConsumMaterialController(IProductionConsumMaterialService productionConsumMaterialService)
        {
            _productionConsumMaterialService = productionConsumMaterialService;
        }


        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("Info")]
        public async Task<IDictionary<long, List<ProductionConsumMaterialModel>>> GetConsumMaterials([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromBody] long[] productionStepIds)
        {
            return await _productionConsumMaterialService.GetConsumMaterials(departmentId, productionOrderId, productionStepIds);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateConsumMaterial([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId
            , [FromBody] ProductionConsumMaterialModel model)
        {
            return await _productionConsumMaterialService.CreateConsumMaterial(departmentId, productionOrderId, productionStepId, model);
        }

        [HttpPut]
        [Route("{productionConsumMaterialId}")]
        public async Task<bool> UpdateConsumMaterial([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId
          , [FromRoute] long productionConsumMaterialId
          , [FromBody] ProductionConsumMaterialModel model)
        {
            return await _productionConsumMaterialService.UpdateConsumMaterial(departmentId, productionOrderId, productionStepId, productionConsumMaterialId, model);
        }

        [HttpDelete]
        [Route("{productionConsumMaterialId}")]
        public async Task<bool> DeleteConsumMaterial([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId
        , [FromRoute] long productionConsumMaterialId)
        {
            return await _productionConsumMaterialService.DeleteConsumMaterial(departmentId, productionOrderId, productionStepId, productionConsumMaterialId);
        }

        [HttpDelete]
        [Route("Material")]
        public async Task<bool> DeleteMaterial([FromQuery] int departmentId, [FromQuery] long productionOrderId, [FromQuery] long productionStepId
        , [FromQuery] int objectTypeId, [FromQuery] long objectId)
        {
            return await _productionConsumMaterialService.DeleteMaterial(departmentId, productionOrderId, productionStepId, objectTypeId, objectId);
        }
    }
}
