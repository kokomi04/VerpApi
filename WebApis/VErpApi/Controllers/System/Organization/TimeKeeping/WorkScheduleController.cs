using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/timekeeping/workSchedule")]
    public class WorkScheduleController : VErpBaseController
    {
        private readonly IWorkScheduleService _workScheduleService;

        public WorkScheduleController(IWorkScheduleService workSchedule)
        {
            _workScheduleService = workSchedule;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> AddWorkSchedule([FromBody] WorkScheduleModel model)
        {
            return await _workScheduleService.AddWorkSchedule(model);
        }

        [HttpDelete]
        [Route("{workScheduleId}")]
        public async Task<bool> DeleteWorkSchedule([FromRoute] int workScheduleId)
        {
            return await _workScheduleService.DeleteWorkSchedule(workScheduleId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<WorkScheduleModel>> GetListWorkSchedule()
        {
            return await _workScheduleService.GetListWorkSchedule();
        }

        [HttpGet]
        [Route("{workScheduleId}")]
        public async Task<WorkScheduleModel> GetWorkSchedule([FromRoute] int workScheduleId)
        {
            return await _workScheduleService.GetWorkSchedule(workScheduleId);
        }

        [HttpPut]
        [Route("{workScheduleId}")]
        public async Task<bool> UpdateWorkSchedule([FromRoute] int workScheduleId, [FromBody] WorkScheduleModel model)
        {
            return await _workScheduleService.UpdateWorkSchedule(workScheduleId, model);

        }
    }
}