using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Service.ProductionPlan;
using VErp.Services.Manafacturing.Model.ProductionPlan;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPlanController : VErpBaseController
    {
        private readonly IProductionPlanService _productionPlanService;

        public ProductionPlanController(IProductionPlanService productionPlanService)
        {
            _productionPlanService = productionPlanService;
        }

        [HttpGet]
        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionPlanService.GetProductionPlan(startDate, endDate);
        }

        [HttpPost]
        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan([FromBody] IDictionary<long, List<ProductionWeekPlanModel>> data)
        {
            return await _productionPlanService.UpdateProductionPlan(data);
        }

        [HttpDelete]
        [Route("{productionOrderId}")]
        public async Task<bool> DeleteProductionPlan([FromBody] long productionOrderId)
        {
            return await _productionPlanService.DeleteProductionPlan(productionOrderId);
        }
    }
}
