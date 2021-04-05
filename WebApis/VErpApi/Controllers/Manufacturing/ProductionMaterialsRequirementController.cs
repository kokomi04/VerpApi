using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using VErp.Services.Manafacturing.Service.ProductionOrder;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/manufacturing/[controller]")]
    [ApiController]
    public class ProductionMaterialsRequirementController : VErpBaseController
    {
        private readonly IProductionMaterialsRequirementService _requirementService;

        public ProductionMaterialsRequirementController(IProductionMaterialsRequirementService requirementService)
        {
            _requirementService = requirementService;
        }

        [HttpPost]
        [Route("department/{departmentId}")]
        public async Task<long> AddProductionMaterialsRequirement([FromRoute] int departmentId, [FromBody] ProductionMaterialsRequirementModel model)
        {
            if (model.MaterialsRequirementDetails.Any(d => d.DepartmentId != departmentId)) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép tạo yêu cầu cho đúng tổ được phân công");
            return await _requirementService.AddProductionMaterialsRequirement(model, EnumProductionMaterialsRequirementStatus.Waiting);
        }

        [HttpPost]
        [Route("manager")]
        public async Task<long> AddProductionMaterialsRequirementForManager([FromBody] ProductionMaterialsRequirementModel model)
        {
            return await _requirementService.AddProductionMaterialsRequirement(model, EnumProductionMaterialsRequirementStatus.Accepted);
        }

        [HttpPut]
        [Route("department/{departmentId}/{productionMaterialsRequirementId}")]
        public async Task<bool> UpdateProductionMaterialsRequirement([FromRoute] int departmentId, [FromRoute] long productionMaterialsRequirementId, [FromBody] ProductionMaterialsRequirementModel model)
        {
            if (model != null && model.MaterialsRequirementDetails.Count == 0)
                return await _requirementService.DeleteProductionMaterialsRequirement(productionMaterialsRequirementId);
            if (model.MaterialsRequirementDetails.Any(d => d.DepartmentId != departmentId)) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép sửa yêu cầu cho đúng tổ được phân công");
            return await _requirementService.UpdateProductionMaterialsRequirement(productionMaterialsRequirementId, model);
        }

        [HttpPut]
        [Route("{productionMaterialsRequirementId}/manager")]
        public async Task<bool> UpdateProductionMaterialsRequirementForManager([FromRoute] long productionMaterialsRequirementId, [FromBody] ProductionMaterialsRequirementModel model)
        {
            if (model != null && model.MaterialsRequirementDetails.Count == 0)
                return await _requirementService.DeleteProductionMaterialsRequirement(productionMaterialsRequirementId);
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
        public async Task<ProductionMaterialsRequirementModel> GetProductionMaterialsRequirement([FromRoute] long productionMaterialsRequirementId)
        {
            return await _requirementService.GetProductionMaterialsRequirement(productionMaterialsRequirementId);
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<ProductionMaterialsRequirementDetailSearch>> SearchProductionMaterialsRequirement([FromQuery] long productionOrderId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromBody] Clause filters)
        {
            return await _requirementService.SearchProductionMaterialsRequirement(productionOrderId, keyword, page, size, filters);
        }

        [HttpGet]
        [Route("productionOrder/{productionOrderId}")]
        public async Task<IList<ProductionMaterialsRequirementDetailListModel>> GetProductionMaterialsRequirementByProductionOrder([FromRoute] long productionOrderId)
        {
            return await _requirementService.GetProductionMaterialsRequirementByProductionOrder(productionOrderId);
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
