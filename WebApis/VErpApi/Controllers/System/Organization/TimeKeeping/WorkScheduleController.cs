using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.SystemParameter;
using Services.Organization.Model.TimeKeeping;
using Services.Organization.Service.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
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
        public async Task<long> AddWorkSchedule([FromBody]WorkScheduleModel model)
        {
            return await _workScheduleService.AddWorkSchedule(model);
        }
        
        [HttpDelete]
        [Route("{shiftConfigurationId}")]
        public async Task<bool> DeleteWorkSchedule([FromRoute]int shiftConfigurationId)
        {
            return await _workScheduleService.DeleteWorkSchedule(shiftConfigurationId);
        }
        
        [HttpGet]
        [Route("")]
        public async Task<IList<WorkScheduleModel>> GetListWorkSchedule()
        {
            return await _workScheduleService.GetListWorkSchedule();
        }
        
        [HttpGet]
        [Route("{shiftConfigurationId}")]
        public async Task<WorkScheduleModel> GetWorkSchedule([FromRoute]int shiftConfigurationId)
        {
            return await _workScheduleService.GetWorkSchedule(shiftConfigurationId);
        }
        
        [HttpPut]
        [Route("{shiftConfigurationId}")]
        public async Task<bool> UpdateWorkSchedule([FromRoute] int shiftConfigurationId, [FromBody]WorkScheduleModel model)
        {
            return await _workScheduleService.UpdateWorkSchedule(shiftConfigurationId, model);

        }
    }
}