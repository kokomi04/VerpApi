using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using VErp.Services.Manafacturing.Service.ProductionOrder;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/[controller]")]
    [ApiController]
    public class ProductionMaterialsRequirementController: VErpBaseController
    {
        private readonly IProductionMaterialsRequirementService _requirementService;

        public ProductionMaterialsRequirementController(IProductionMaterialsRequirementService requirementService)
        {
            _requirementService = requirementService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddProductionMaterialsRequirement([FromBody] ProductionMaterialsRequirementModel model)
        {
            return await _requirementService.AddProductionMaterialsRequirement(model);
        }

        [HttpPut]
        [Route("{productionMaterialsRequirementId}")]
        public async Task<bool> UpdateProductionMaterialsRequirement([FromRoute]long productionMaterialsRequirementId, [FromBody]ProductionMaterialsRequirementModel model)
        {
            return await _requirementService.UpdateProductionMaterialsRequirement(productionMaterialsRequirementId, model);
        }

        [HttpDelete]
        [Route("{productionMaterialsRequirementId}")]
        public async Task<bool> DeleteProductionMaterialsRequirement([FromRoute] long productionMaterialsRequirementId)
        {
            return await _requirementService.DeleteProductionMaterialsRequirement(productionMaterialsRequirementId);
        }

        [HttpGet]
        [Route("{productionMaterialsRequirementId}")]
        public async Task<ProductionMaterialsRequirementModel> GetProductionMaterialsRequirement([FromRoute]long productionMaterialsRequirementId)
        {
            return await _requirementService.GetProductionMaterialsRequirement(productionMaterialsRequirementId);
        }

        [HttpPost]
        [Route("search")]
        public async Task<PageData<ProductionMaterialsRequirementDetailSearch>> SearchProductionMaterialsRequirement([FromQuery]string keyword, [FromQuery] int page, [FromQuery] int size,[FromBody] Clause filters)
        {
            return await _requirementService.SearchProductionMaterialsRequirement(keyword, page, size, filters);
        }

        [HttpPut]
        [Route("{productionMaterialsRequirementId}/reject")]
        public async Task<bool> RejectInventoryRequirement([FromRoute] long productionMaterialsRequirementId)
        {
            return await _requirementService.ConfirmInventoryRequirement(productionMaterialsRequirementId, EnumProductionMaterialsRequirementStatus.Rejected);
        }

        [HttpPut]
        [Route("{productionMaterialsRequirementId}/accpet")]
        public async Task<bool> AcceptInventoryRequirement([FromRoute] long productionMaterialsRequirementId)
        {
            return await _requirementService.ConfirmInventoryRequirement(productionMaterialsRequirementId, EnumProductionMaterialsRequirementStatus.Accepted);
        }
    }                                                                                                                         
}
