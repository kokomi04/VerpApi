using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.System.Organization
{
    [Route("api/organization/timekeeping/timesheetraw")]
    public class TimeSheetRawController : VErpBaseController
    {
        private readonly ITimeSheetRawService _timeSheetRawService;

        public TimeSheetRawController(ITimeSheetRawService timeSheetRawService)
        {
            _timeSheetRawService = timeSheetRawService;
        }


        [HttpPost]
        [Route("")]
        public async Task<long> AddTimeSheetRaw([FromBody] TimeSheetRawModel model)
        {
            return await _timeSheetRawService.AddTimeSheetRaw(model);
        }

        [HttpDelete]
        [Route("{timeSheetRawId}")]
        public async Task<bool> DeleteTimeSheetRaw([FromRoute] long timeSheetRawId)
        {
            return await _timeSheetRawService.DeleteTimeSheetRaw(timeSheetRawId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<TimeSheetRawModel>> GetListTimeSheetRaw()
        {
            return await _timeSheetRawService.GetListTimeSheetRaw();
        }

        [HttpGet]
        [Route("{timeSheetRawId}")]
        public async Task<TimeSheetRawModel> GetTimeSheetRaw([FromRoute] long timeSheetRawId)
        {
            return await _timeSheetRawService.GetTimeSheetRaw(timeSheetRawId);
        }

        [HttpPut]
        [Route("{timeSheetRawId}")]
        public async Task<bool> UpdateTimeSheetRaw([FromRoute] long timeSheetRawId, [FromBody] TimeSheetRawModel model)
        {
            return await _timeSheetRawService.UpdateTimeSheetRaw(timeSheetRawId, model);

        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetFieldDataForMapping()
        {
            return _timeSheetRawService.GetFieldDataForMapping();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _timeSheetRawService.ImportTimeSheetRawFromMapping(mapping, file.OpenReadStream()).ConfigureAwait(true);
        }


    }
}