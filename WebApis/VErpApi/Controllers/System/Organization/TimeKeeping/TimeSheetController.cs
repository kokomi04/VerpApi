using Microsoft.AspNetCore.Http;
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
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/timekeeping/timesheet")]
    public class TimeSheetController: VErpBaseController
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

        [HttpGet]
        [Route("fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping()
        {
            return await _timeSheetService.GetFieldDataForMapping();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromQuery] int month, [FromQuery] int year, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _timeSheetService.ImportTimeSheetFromMapping(month, year, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }


    }
}