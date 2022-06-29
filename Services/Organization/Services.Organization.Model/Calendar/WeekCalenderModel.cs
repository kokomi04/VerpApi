using System.Collections.Generic;

namespace VErp.Services.Organization.Model.Calendar

{
    public class WeekCalendarModel
    {
        public long? StartDate { get; set; }
        public double WorkingHourPerDay { get; set; }
        public ICollection<WorkingWeekInfoModel> WorkingWeek { get; set; }
        public WeekCalendarModel()
        {
            WorkingWeek = new List<WorkingWeekInfoModel>();
        }
    }
}
