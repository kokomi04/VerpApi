using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Organization.Model.Calendar;

namespace VErp.Services.Organization.Service.Calendar
{
    public interface ICalendarService
    {
        Task<WeekCalendarModel> GetCurrentCalendar();
        Task<IList<WeekCalendarModel>> GetCalendar();
        Task<WeekCalendarModel> UpdateWeekCalendar(WeekCalendarModel data);

        Task<bool> DeleteWeekCalendar(long startDate);

        Task<IList<DayOffCalendarModel>> GetDayOffCalendar(long startDate, long endDate);

        Task<DayOffCalendarModel> UpdateDayOff(DayOffCalendarModel data);

        Task<bool> DeleteDayOff(long day);
    }
}
