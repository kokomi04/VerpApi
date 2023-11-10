using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Employee;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.Organization.TimeKeeping
{
    [Route("api/organization/timekeeping/overtimePlan")]
    public class OvertimePlanController : VErpBaseController
    {
        private readonly IOvertimePlanService _overtimePlanService;

        public OvertimePlanController(IOvertimePlanService overtimePlanService)
        {
            _overtimePlanService = overtimePlanService;
        }


        [HttpPost]
        [Route("")]
        public async Task<bool> AddOvertimePlan([FromBody] OvertimePlanRequestModel model)
        {
            return await _overtimePlanService.AddOvertimePlan(model);
        }

        [HttpDelete]
        [Route("")]
        public async Task<bool> DeleteOvertimePlan([FromBody] OvertimePlanRequestModel model)
        {
            return await _overtimePlanService.DeleteOvertimePlan(model);
        }

        [HttpPost]
        [Route("Search")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<OvertimePlanModel>> GetListOvertimePlan([FromBody] OvertimePlanRequestModel model)
        {
            return await _overtimePlanService.GetListOvertimePlan(model);
        }
    }
}