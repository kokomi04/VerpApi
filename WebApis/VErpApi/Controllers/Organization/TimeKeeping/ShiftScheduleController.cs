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
    [Route("api/organization/timekeeping/shiftschedule")]
    public class ShiftScheduleController : VErpBaseController
    {
        private readonly IShiftScheduleService _shiftScheduleService;

        public ShiftScheduleController(IShiftScheduleService shiftScheduleService)
        {
            _shiftScheduleService = shiftScheduleService;
        }


        [HttpPost]
        [Route("")]
        public async Task<long> AddShiftSchedule([FromBody] ShiftScheduleModel model)
        {
            return await _shiftScheduleService.AddShiftSchedule(model);
        }

        [HttpDelete]
        [Route("{shiftScheduleId}")]
        public async Task<bool> DeleteShiftSchedule([FromRoute] long shiftScheduleId)
        {
            return await _shiftScheduleService.DeleteShiftSchedule(shiftScheduleId);
        }

        [HttpPost]
        [Route("Search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<ShiftScheduleModel>> GetListShiftSchedule([FromBody] ShiftScheduleRequestModel request)
        {
            if (request == null) 
                throw new BadRequestException(GeneralCode.InvalidParams);

            return await _shiftScheduleService.GetListShiftSchedule(request, request.Page, request.Size);
        }

        [HttpGet]
        [Route("{shiftScheduleId}")]
        public async Task<ShiftScheduleModel> GetShiftSchedule([FromRoute] long shiftScheduleId)
        {
            return await _shiftScheduleService.GetShiftSchedule(shiftScheduleId);
        }

        [HttpPut]
        [Route("{shiftScheduleId}")]
        public async Task<bool> UpdateShiftSchedule([FromRoute] long shiftScheduleId, [FromBody] ShiftScheduleModel model)
        {
            return await _shiftScheduleService.UpdateShiftSchedule(shiftScheduleId, model);

        }

        [HttpPost("employees")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<NonCamelCaseDictionary>> GetEmployeesByDepartments([FromBody] List<int> departmentIds)
        {
            return await _shiftScheduleService.GetEmployeesByDepartments(departmentIds);
        }

        [HttpPost("notAssignedEmployees")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<NonCamelCaseDictionary>> GetNotAssignedEmployees()
        {
            return await _shiftScheduleService.GetNotAssignedEmployees();
        }

        [HttpPost("warnings")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<EmployeeViolationModel>> GetListEmployeeViolations()
        {
            return await _shiftScheduleService.GetListEmployeeViolations();
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping()
        {
            return await _shiftScheduleService.GetFieldDataForMapping();
        }

        [HttpPost]
        [Route("{shiftScheduleId}/importFromMapping")]
        public async Task<List<ShiftScheduleDetailModel>> ImportFromMapping([FromRoute] long shiftScheduleId, [FromQuery] long fromDate, [FromQuery] long toDate, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _shiftScheduleService.ImportShiftScheduleFromMapping(shiftScheduleId, fromDate, toDate, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }
    }
}