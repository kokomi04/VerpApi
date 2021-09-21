using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.DepartmentCalendar;
using VErp.Services.Organization.Service.DepartmentCalendar;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalDepartmentCalendarController : CrossServiceBaseController
    {
        private readonly IDepartmentCalendarService _departmentCalendarService;

        public InternalDepartmentCalendarController(IDepartmentCalendarService departmentCalendarService)
        {
            _departmentCalendarService = departmentCalendarService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("multiple")]
        public async Task<IList<DepartmentCalendarListModel>> GetListDepartmentCalendar([FromQuery] long startDate, [FromQuery] long endDate, [FromBody] int[] departmentIds)
        {
            return await _departmentCalendarService.GetListDepartmentCalendar(departmentIds, startDate, endDate);
        }
    }
}