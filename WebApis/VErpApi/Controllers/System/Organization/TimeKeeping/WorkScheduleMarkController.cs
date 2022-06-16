using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/timekeeping/workScheduleMark")]
    public class WorkScheduleMarkController : VErpBaseController
    {
        private readonly IWorkScheduleMarkService _workScheduleMarkService;

        public WorkScheduleMarkController(IWorkScheduleMarkService workSchedule)
        {
            _workScheduleMarkService = workSchedule;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddWorkScheduleMark([FromBody] WorkScheduleMarkModel model)
        {
            return await _workScheduleMarkService.AddWorkScheduleMark(model);
        }

        [HttpDelete]
        [Route("{workScheduleMarkId}")]
        public async Task<bool> DeleteWorkScheduleMark([FromRoute] int workScheduleMarkId)
        {
            return await _workScheduleMarkService.DeleteWorkScheduleMark(workScheduleMarkId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<WorkScheduleMarkModel>> GetListWorkScheduleMark([FromQuery] int? employeeId)
        {
            return await _workScheduleMarkService.GetListWorkScheduleMark(employeeId);
        }

        [HttpGet]
        [Route("{workScheduleMarkId}")]
        public async Task<WorkScheduleMarkModel> GetWorkScheduleMark([FromRoute] int workScheduleMarkId)
        {
            return await _workScheduleMarkService.GetWorkScheduleMark(workScheduleMarkId);
        }

        [HttpPut]
        [Route("{workScheduleMarkId}")]
        public async Task<bool> UpdateWorkScheduleMark([FromRoute] int workScheduleMarkId, [FromBody] WorkScheduleMarkModel model)
        {
            return await _workScheduleMarkService.UpdateWorkScheduleMark(workScheduleMarkId, model);

        }
    }
}