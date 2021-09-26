using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Commons.GlobalObject;
using VErp.Services.Manafacturing.Service.ProductionPlan;
using VErp.Services.Manafacturing.Model.ProductionPlan;

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
