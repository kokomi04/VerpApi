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
    public class ProductionHandoverController : VErpBaseController
    {
        private readonly IProductionHandoverService _productionHandoverService;

        public ProductionHandoverController(IProductionHandoverService productionHandoverService)
        {
            _productionHandoverService = productionHandoverService;
        }

        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<IList<ProductionHandoverModel>> GetProductionHandovers([FromRoute] long productionOrderId)
        {
            return await _productionHandoverService.GetProductionHandovers(productionOrderId);
        }

        [HttpGet]
        [Route("{productionOrderId}/productionStep/{productionStepId}/department/{departmentId}")]
        public async Task<DepartmentHandoverDetailModel> GetDepartmentHandoverDetail([FromRoute] long productionOrderId, [FromRoute] long productionStepId, [FromRoute] long departmentId)
        {
            return await _productionHandoverService.GetDepartmentHandoverDetail(productionOrderId, productionStepId, departmentId);
        }

        [HttpPost]
        [Route("DepartmentHandover/{departmentId}")]
        public async Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers([FromRoute] long departmentId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] long fromDate, [FromQuery] long toDate)
        {
            return await _productionHandoverService.GetDepartmentHandovers(departmentId, keyword, page, size, fromDate, toDate);
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

        [HttpDelete]
        [Route("{productionHandoverId}")]
        public async Task<bool> DeleteProductionHandover([FromRoute] long productionHandoverId)
        {
            return await _productionHandoverService.DeleteProductionHandover(productionHandoverId);
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
    }
}
