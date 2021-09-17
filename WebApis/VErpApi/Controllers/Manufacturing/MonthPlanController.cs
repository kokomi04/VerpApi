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
using VErp.Services.Manafacturing.Model.ProductionPlan;
using VErp.Services.Manafacturing.Service.ProductionPlan;
using VErp.Commons.GlobalObject;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonthPlanController : VErpBaseController
    {
        private readonly IMonthPlanService _monthPlanService;

        public MonthPlanController(IMonthPlanService monthPlanService)
        {
            _monthPlanService = monthPlanService;
        }

        [HttpPost]
        [Route("")]
        public async Task<MonthPlanModel> CreateMonthPlan([FromBody] MonthPlanModel data)
        {
            return await _monthPlanService.CreateMonthPlan(data);
        }


        [HttpPost]
        [Route("Search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<MonthPlanModel>> GetMonthPlans([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _monthPlanService.GetMonthPlans(keyword, page, size, orderByFieldName, asc, filters);
        }

        [HttpPut]
        [Route("{monthPlanId}")]
        public async Task<MonthPlanModel> UpdateMonthPlan([FromRoute] int monthPlanId, [FromBody] MonthPlanModel data)
        {
            return await _monthPlanService.UpdateMonthPlan(monthPlanId, data);
        }

        [HttpGet]
        [Route("{monthPlanId}")]
        public async Task<MonthPlanModel> GetMonthPlan([FromRoute] int monthPlanId)
        {
            return await _monthPlanService.GetMonthPlan(monthPlanId);
        }

        [HttpGet]
        [Route("start/{startDate}/end/{endDate}")]
        public async Task<MonthPlanModel> GetMonthPlan([FromRoute] long startDate, [FromRoute] long endDate)
        {
            return await _monthPlanService.GetMonthPlan(startDate, endDate);
        }

        [HttpGet]
        [Route("name/{monthPlanName}")]
        public async Task<MonthPlanModel> GetMonthPlan([FromRoute] string monthPlanName)
        {
            return await _monthPlanService.GetMonthPlan(monthPlanName);
        }

        [HttpDelete]
        [Route("{monthPlanId}")]
        public async Task<bool> DeleteMonthPlan([FromRoute] int monthPlanId)
        {
            return await _monthPlanService.DeleteMonthPlan(monthPlanId);
        }
    }
}
