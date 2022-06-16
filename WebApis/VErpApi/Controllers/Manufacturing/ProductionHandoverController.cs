using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionHandoverController : VErpBaseController
    {
        private readonly IProductionHandoverService _productionHandoverService;

        public ProductionHandoverController(IProductionHandoverService productionHandoverService)
        {
            _productionHandoverService = productionHandoverService;
        }

        [HttpGet]
        [Route("DepartmentHandoverByDate")]
        public async Task<PageData<ProductionHandoverByDateModel>> GetDepartmentHandovers([FromQuery] IList<long> fromDepartmentIds, [FromQuery] IList<long> toDepartmentIds, [FromQuery] IList<long> fromStepIds, [FromQuery] IList<long> toStepIds, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] bool? isInFinish, [FromQuery] bool? isOutFinish, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productionHandoverService.GetDepartmentHandoversByDate(fromDepartmentIds, toDepartmentIds, fromStepIds, toStepIds, fromDate, toDate, isInFinish, isOutFinish, page, size);
        }


        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<IList<ProductionHandoverModel>> GetProductionHandovers([FromRoute] long productionOrderId)
        {
            return await _productionHandoverService.GetProductionHandovers(productionOrderId);
        }

        [HttpGet]
        [Route("{productionOrderId}/productionStep/{productionStepId}/department/{departmentId}")]
        public async Task<Dictionary<long, DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromRoute] int departmentId)
        {
            var lstDetail = await _productionHandoverService.GetDepartmentHandoverDetail(productionOrderId, productionStepId, departmentId);
            var group = lstDetail.GroupBy(d => d.ProductionStepId).ToDictionary(g => g.Key, g => g.First());
            return group;
        }

        [HttpPost]
        [Route("GetDetailByArrayProductionOrder/department/{departmentId}")]
        public async Task<IList<DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail([FromBody] IList<RequestObjectGetProductionOrderHandover> data, [FromRoute] int departmentId)
        {
            var lstDetail = await _productionHandoverService.GetDepartmentHandoverDetailByArrayProductionOrderId(data, departmentId);
            return lstDetail;
        }

        [HttpPost]
        [Route("DepartmentHandover/{departmentId}")]
        public async Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers([FromRoute] long departmentId, [FromQuery] string keyword, [FromQuery] int? stepId, [FromQuery] int? productId, [FromQuery] int page, [FromQuery] int size, [FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] bool? isInFinish, [FromQuery] bool? isOutFinish, [FromQuery] EnumProductionStepLinkDataRoleType? productionStepLinkDataRoleTypeId)
        {
            return await _productionHandoverService.GetDepartmentHandovers(departmentId, keyword, page, size, fromDate, toDate, stepId, productId, isInFinish, isOutFinish, productionStepLinkDataRoleTypeId);
        }




        [HttpGet]
        [Route("productionInventoryRequirement/{productionOrderId}")]
        public async Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements([FromRoute] long productionOrderId)
        {
            return await _productionHandoverService.GetProductionInventoryRequirements(productionOrderId);
        }

        [HttpPost]
        [Route("{productionOrderId}")]
        public async Task<ProductionHandoverModel> CreateProductionHandover([FromRoute] long productionOrderId, [FromBody] ProductionHandoverInputModel data)
        {
            return await _productionHandoverService.CreateProductionHandover(productionOrderId, data);
        }

        [HttpPost]
        [Route("statictic/{productionOrderId}")]
        public async Task<ProductionHandoverModel> CreateStatictic([FromRoute] long productionOrderId, [FromBody] ProductionHandoverInputModel data)
        {
            return await _productionHandoverService.CreateStatictic(productionOrderId, data);
        }

        [HttpPost]
        [Route("statictic/multiple/{productionOrderId}")]
        public async Task<IList<ProductionHandoverModel>> CreateMultipleStatictic([FromRoute] long productionOrderId, [FromBody] IList<ProductionHandoverInputModel> data)
        {
            return await _productionHandoverService.CreateMultipleStatictic(productionOrderId, data);
        }

        [HttpDelete]
        [Route("{productionHandoverId}")]
        public async Task<bool> DeleteProductionHandover([FromRoute] long productionHandoverId)
        {
            return await _productionHandoverService.DeleteProductionHandover(productionHandoverId);
        }

        [HttpPut]
        [Route("AcceptBatch")]
        public async Task<bool> AcceptBatch([FromBody] IList<ProductionHandoverAcceptBatchInput> req)
        {
            return await _productionHandoverService.AcceptProductionHandoverBatch(req);
        }

        [HttpPut]
        [Route("{productionOrderId}/{productionHandoverId}/accept")]
        public async Task<ProductionHandoverModel> AcceptProductionHandover([FromRoute] long productionOrderId, [FromRoute] long productionHandoverId)
        {
            return await _productionHandoverService.ConfirmProductionHandover(productionOrderId, productionHandoverId, EnumHandoverStatus.Accepted);
        }

        [HttpPut]
        [Route("{productionOrderId}/{productionHandoverId}/reject")]
        public async Task<ProductionHandoverModel> RejectProductionHandover([FromRoute] long productionOrderId, [FromRoute] long productionHandoverId)
        {
            return await _productionHandoverService.ConfirmProductionHandover(productionOrderId, productionHandoverId, EnumHandoverStatus.Rejected);
        }

        [HttpPost]
        [Route("patch")]
        public async Task<bool> CreateProductionHandoverPatch([FromBody] IList<ProductionHandoverInputModel> data)
        {
            return await _productionHandoverService.CreateProductionHandoverPatch(data);
        }
    }
}
