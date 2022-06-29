using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionPlan;
using VErp.Services.Manafacturing.Service.ProductionPlan;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPlanExtraInfoController : VErpBaseController
    {
        private readonly IProductionPlanExtraInfoService _productionPlanExtraInfoService;

        public ProductionPlanExtraInfoController(IProductionPlanExtraInfoService productionPlanExtraInfoService)
        {
            _productionPlanExtraInfoService = productionPlanExtraInfoService;
        }

        [HttpPut]
        [Route("{monthPlanId}")]
        public async Task<IList<ProductionPlanExtraInfoModel>> UpdateProductionPlanExtraInfo([FromRoute] int monthPlanId, [FromBody] IList<ProductionPlanExtraInfoModel> data)
        {
            return await _productionPlanExtraInfoService.UpdateProductionPlanExtraInfo(monthPlanId, data);
        }

        [HttpGet]
        [Route("{monthPlanId}")]
        public async Task<IList<ProductionPlanExtraInfoModel>> GetProductionPlanExtraInfo([FromRoute] int monthPlanId)
        {
            return await _productionPlanExtraInfoService.GetProductionPlanExtraInfo(monthPlanId);
        }
    }
}
