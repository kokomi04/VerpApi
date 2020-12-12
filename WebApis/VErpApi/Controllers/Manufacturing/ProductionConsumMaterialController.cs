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
    public class ProductionConsumMaterialController : VErpBaseController
    {
        private readonly IProductionConsumMaterialService _productionConsumMaterialService;

        public ProductionConsumMaterialController(IProductionConsumMaterialService productionConsumMaterialService)
        {
            _productionConsumMaterialService = productionConsumMaterialService;
        }


        [HttpGet]
        [Route("")]
        public async Task<IList<ProductionConsumMaterialModel>> GetConsumMaterials([FromQuery] int departmentId, [FromQuery] long scheduleTurnId, [FromQuery] long productionStepId)
        {
            return await _productionConsumMaterialService.GetConsumMaterials(departmentId, scheduleTurnId, productionStepId);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateConsumMaterial([FromQuery] int departmentId, [FromQuery] long scheduleTurnId, [FromQuery] long productionStepId
            , [FromBody] ProductionConsumMaterialModel model)
        {
            return await _productionConsumMaterialService.CreateConsumMaterial(departmentId, scheduleTurnId, productionStepId, model);
        }

        [HttpPut]
        [Route("{productionConsumMaterialId}")]
        public async Task<bool> UpdateConsumMaterial([FromQuery] int departmentId, [FromQuery] long scheduleTurnId, [FromQuery] long productionStepId
          , [FromRoute] long productionConsumMaterialId
          , [FromBody] ProductionConsumMaterialModel model)
        {
            return await _productionConsumMaterialService.UpdateConsumMaterial(departmentId, scheduleTurnId, productionStepId, productionConsumMaterialId, model);
        }

        [HttpDelete]
        [Route("{productionConsumMaterialId}")]
        public async Task<bool> DeleteConsumMaterial([FromQuery] int departmentId, [FromQuery] long scheduleTurnId, [FromQuery] long productionStepId
        , [FromRoute] long productionConsumMaterialId)
        {
            return await _productionConsumMaterialService.DeleteConsumMaterial(departmentId, scheduleTurnId, productionStepId, productionConsumMaterialId);
        }

    }
}
