using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Service.Department;
using VErp.Services.Organization.Model.Department;
using System.Collections.Generic;
using VErp.Services.Stock.Service.FileResources;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Organization.Service.Calendar;
using VErp.Services.Organization.Model.Calendar;

namespace VErpApi.Controllers.System
{
    [Route("api/calendar")]
    public class CalendarController : VErpBaseController
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet]
        [Route("")]
        public async Task<WeekCalendarModel> GetCurrentCalendar()
        {
            return await _calendarService.GetCurrentCalendar();
        }


        [HttpPut]
        [Route("")]
        public async Task<WeekCalendarModel> UpdateWeekCalendar([FromBody] WeekCalendarModel data)
        {
            return await _calendarService.UpdateWeekCalendar(data);
        }

        [HttpGet]
        [Route("dayoff")]
        public async Task<IList<DayOffCalendarModel>> GetDayOffCalendar([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _calendarService.GetDayOffCalendar(startDate, endDate);
        }

        [HttpPost]
        [Route("dayoff")]
        public async Task<DayOffCalendarModel> InsertDayOff( [FromBody] DayOffCalendarModel data)
        {
            return await _calendarService.UpdateDayOff(data);
        }

        [HttpDelete]
        [Route("dayoff/{day}")]
        public async Task<bool> DeleteDayOff([FromRoute] long day)
        {
            return await _calendarService.DeleteDayOff(day);
        }

    }
}