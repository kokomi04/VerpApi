using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Calendar;
using VErp.Services.Organization.Service.Calendar;

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
        public async Task<PageData<CalendarModel>> GetListCalendar([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _calendarService.GetListCalendar(keyword, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<CalendarModel> AddCalendar([FromBody] CalendarModel data)
        {
            return await _calendarService.AddCalendar(data);
        }

        [HttpPut]
        [Route("{calendarId}")]
        public async Task<CalendarModel> UpdateCalendar([FromRoute] int calendarId, [FromBody] CalendarModel data)
        {
            return await _calendarService.UpdateCalendar(calendarId, data);
        }

        [HttpDelete]
        [Route("{calendarId}")]
        public async Task<bool> DeleteCalendar([FromRoute] int calendarId)
        {
            return await _calendarService.DeleteCalendar(calendarId);
        }

        [HttpPost]
        [Route("clone/{calendarId}")]
        public async Task<CalendarModel> CloneCalendar([FromRoute] int calendarId)
        {
            return await _calendarService.CloneCalendar(calendarId);
        }

        [HttpGet]
        [Route("{calendarId}/info")]
        public async Task<WeekCalendarModel> GetCurrentCalendar([FromRoute] int calendarId)
        {
            return await _calendarService.GetCurrentCalendar(calendarId);
        }

        [HttpGet]
        [Route("{calendarId}/info/all")]
        public async Task<IList<WeekCalendarModel>> GetCalendar([FromRoute] int calendarId)
        {
            return await _calendarService.GetCalendar(calendarId);
        }

        [HttpPost]
        [Route("{calendarId}/info")]
        public async Task<WeekCalendarModel> CreateWeekCalendar([FromRoute] int calendarId, [FromBody] WeekCalendarModel data)
        {
            return await _calendarService.CreateWeekCalendar(calendarId, data);
        }

        [HttpPut]
        [Route("{calendarId}/info/{oldDate}")]
        public async Task<WeekCalendarModel> UpdateWeekCalendar([FromRoute] int calendarId, [FromRoute] long oldDate, [FromBody] WeekCalendarModel data)
        {
            return await _calendarService.UpdateWeekCalendar(calendarId, oldDate, data);
        }

        [HttpDelete]
        [Route("{calendarId}/info/{startDate}")]
        public async Task<bool> DeleteWeekCalendar([FromRoute] int calendarId, [FromRoute] long startDate)
        {
            return await _calendarService.DeleteWeekCalendar(calendarId, startDate);
        }

        [HttpGet]
        [Route("{calendarId}/info/dayoff")]
        public async Task<IList<DayOffCalendarModel>> GetDayOffCalendar([FromRoute] int calendarId, [FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _calendarService.GetDayOffCalendar(calendarId, startDate, endDate);
        }

        [HttpPost]
        [Route("{calendarId}/info/dayoff")]
        public async Task<DayOffCalendarModel> InsertDayOff([FromRoute] int calendarId, [FromBody] DayOffCalendarModel data)
        {
            return await _calendarService.UpdateDayOff(calendarId, data);
        }

        [HttpDelete]
        [Route("{calendarId}/info/dayoff/{day}")]
        public async Task<bool> DeleteDayOff([FromRoute] int calendarId, [FromRoute] long day)
        {
            return await _calendarService.DeleteDayOff(calendarId, day);
        }

    }
}