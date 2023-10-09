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
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.TimeKeeping;

namespace VErpApi.Controllers.Organization.TimeKeeping
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

        [HttpPost]
        [Route("Search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<TimeSheetRawViewModel>> GetListTimeSheetRaw([FromBody] TimeSheetRawRequestModel request)
        {
            if (request == null) 
                throw new BadRequestException(GeneralCode.InvalidParams);

            return await _timeSheetRawService.GetListTimeSheetRaw(request, request.Page, request.Size);
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
        [Route("distinct")]
        public async Task<IList<TimeSheetRawModel>> GetDistinctTimeSheetRawByEmployee([FromBody] List<long?> employeeIds)
        {
            return await _timeSheetRawService.GetDistinctTimeSheetRawByEmployee(employeeIds);
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping()
        {
            return await _timeSheetRawService.GetFieldDataForMapping();
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

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] TimeSheetRawExportModel req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            var (stream, fileName, contentType) = await _timeSheetRawService.Export(req);
            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }
    }
}