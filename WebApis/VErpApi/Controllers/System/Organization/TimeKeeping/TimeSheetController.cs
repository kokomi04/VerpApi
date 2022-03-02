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
        [Route("{year}/{month}")]
        public async Task<bool> DeleteTimeSheet([FromRoute] int year, [FromRoute] int month)
        {
            return await _timeSheetService.DeleteTimeSheet(year, month);
        }
        
        [HttpGet]
        [Route("")]
        public async Task<IList<TimeSheetModel>> GetListTimeSheet()
        {
            return await _timeSheetService.GetListTimeSheet();
        }
        
        [HttpGet]
        [Route("{year}/{month}")]
        public async Task<TimeSheetModel> GetTimeSheet([FromRoute]int year, [FromRoute] int month)
        {
            return await _timeSheetService.GetTimeSheet(year, month);
        }
        
        [HttpPut]
        [Route("{year}/{month}")]
        public async Task<bool> UpdateTimeSheet([FromRoute] int year, [FromRoute] int month, [FromBody]TimeSheetModel model)
        {
            return await _timeSheetService.UpdateTimeSheet(year, month, model);

        }

        [HttpPut]
        [Route("{year}/{month}/approve")]
        public async Task<bool> ApproveTimeSheet([FromRoute] int year, [FromRoute] int month)
        {
            return await _timeSheetService.ApproveTimeSheet(year, month);

        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetFieldDataForMapping([FromQuery] long beginDate,[FromQuery] long endDate)
        {
            return _timeSheetService.GetFieldDataForMapping(beginDate, endDate);
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromQuery] int month, [FromQuery] int year,[FromQuery] long beginDate, [FromQuery] long endDate, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _timeSheetService.ImportTimeSheetFromMapping(month, year,beginDate, endDate, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }


    }
}