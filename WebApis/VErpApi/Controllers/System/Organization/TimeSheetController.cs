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
    [Route("api/organization/timekeeping/timesheet")]
    public class TimeSheetController
    {
        private readonly ITimeSheetService _timeSheetService;

        public TimeSheetController(ITimeSheetService timeSheetService)
        {
            _timeSheetService = timeSheetService;
        }

        
        [HttpPost]
        [Route("")]
        public async Task<long> AddTimeSheet([FromBody]TimeSheetModel model)
        {
            return await _timeSheetService.AddTimeSheet(model);
        }
        
        [HttpDelete]
        [Route("{timeSheetId}")]
        public async Task<bool> DeleteTimeSheet([FromRoute]long timeSheetId)
        {
            return await _timeSheetService.DeleteTimeSheet(timeSheetId);
        }
        
        [HttpGet]
        [Route("")]
        public async Task<IList<TimeSheetModel>> GetListTimeSheet()
        {
            return await _timeSheetService.GetListTimeSheet();
        }
        
        [HttpGet]
        [Route("{timeSheetId}")]
        public async Task<TimeSheetModel> GetTimeSheet([FromRoute]long timeSheetId)
        {
            return await _timeSheetService.GetTimeSheet(timeSheetId);
        }
        
        [HttpPut]
        [Route("{timeSheetId}")]
        public async Task<bool> UpdateTimeSheet([FromRoute] long timeSheetId, [FromBody]TimeSheetModel model)
        {
            return await _timeSheetService.UpdateTimeSheet(timeSheetId, model);

        }
    }
}