using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Calendar;

namespace VErp.Services.Organization.Service.Calendar
{
    public interface ICalendarService
    {
        Task<PageData<CalendarModel>> GetListCalendar(string keyword, int page, int size, Clause filter = null);

        Task<CalendarModel> AddCalendar(CalendarModel data);
        Task<CalendarModel> UpdateCalendar(int calendarId, CalendarModel data);
        Task<bool> DeleteCalendar(int calendarId);

        Task<WeekCalendarModel> GetCurrentCalendar(int calendarId);
        Task<IList<WeekCalendarModel>> GetCalendar(int calendarId);
        Task<IList<DayOffCalendarModel>> GetDayOffCalendar(int calendarId, long startDate, long endDate);

        Task<WeekCalendarModel> CreateWeekCalendar(int calendarId, WeekCalendarModel data);
        Task<WeekCalendarModel> UpdateWeekCalendar(int calendarId, long oldDate, WeekCalendarModel data);
        Task<bool> DeleteWeekCalendar(int calendarId, long startDate);
        Task<DayOffCalendarModel> UpdateDayOff(int calendarId, DayOffCalendarModel data);
        Task<bool> DeleteDayOff(int calendarId, long day);
    }
}
