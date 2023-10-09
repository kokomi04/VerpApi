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
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.Organization.TimeKeeping
{
    [Route("api/organization/timekeeping/timesheet")]
    public class TimeSheetController : VErpBaseController
    {
        private readonly ITimeSheetService _timeSheetService;

        public TimeSheetController(ITimeSheetService timeSheetService)
        {
            _timeSheetService = timeSheetService;
        }


        [HttpPost]
        [Route("")]
        public async Task<long> AddTimeSheet([FromBody] TimeSheetModel model)
        {
            return await _timeSheetService.AddTimeSheet(model);
        }

        [HttpDelete]
        [Route("{timeSheetId}")]
        public async Task<bool> DeleteTimeSheet([FromRoute] long timeSheetId)
        {
            return await _timeSheetService.DeleteTimeSheet(timeSheetId);
        }

        [HttpPost]
        [Route("Search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<TimeSheetModel>> GetListTimeSheet([FromBody] TimeSheetRequestModel request)
        {
            return await _timeSheetService.GetListTimeSheet(request, request.Page, request.Size);
        }

        [HttpGet]
        [Route("{timeSheetId}")]
        public async Task<TimeSheetModel> GetTimeSheet([FromRoute] long timeSheetId)
        {
            return await _timeSheetService.GetTimeSheet(timeSheetId);
        }

        [HttpGet]
        [Route("{timeSheetId}/employee/{employeeId}")]
        public async Task<TimeSheetModel> GetTimeSheetByEmployee([FromRoute] long timeSheetId, [FromRoute] int employeeId)
        {
            return await _timeSheetService.GetTimeSheetByEmployee(timeSheetId, employeeId);
        }

        [HttpPut]
        [Route("{timeSheetId}")]
        public async Task<bool> UpdateTimeSheet([FromRoute] long timeSheetId, [FromBody] TimeSheetModel model)
        {
            return await _timeSheetService.UpdateTimeSheet(timeSheetId, model);

        }

        [HttpPut]
        [Route("{timeSheetId}/approve")]
        public async Task<bool> ApproveTimeSheet([FromRoute] long timeSheetId)
        {
            return await _timeSheetService.ApproveTimeSheet(timeSheetId);

        }

        //[HttpGet]
        //[Route("fieldDataForMapping")]
        //public CategoryNameModel GetFieldDataForMapping([FromQuery] long beginDate, [FromQuery] long endDate)
        //{
        //    return _timeSheetService.GetFieldDataForMapping(beginDate, endDate);
        //}

        //[HttpPost]
        //[Route("importFromMapping")]
        //public async Task<bool> ImportFromMapping([FromQuery] int month, [FromQuery] int year, [FromQuery] long beginDate, [FromQuery] long endDate, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        //{
        //    if (file == null)
        //    {
        //        throw new BadRequestException(GeneralCode.InvalidParams);
        //    }
        //    return await _timeSheetService.ImportTimeSheetFromMapping(month, year, beginDate, endDate, mapping, file.OpenReadStream()).ConfigureAwait(true);
        //}


    }
}