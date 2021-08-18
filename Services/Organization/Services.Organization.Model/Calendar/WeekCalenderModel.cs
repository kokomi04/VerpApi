using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Organization.Model.Calendar

{
    public class WeekCalendarModel
    {
        public double WorkingHourPerDay { get; set; }
        public ICollection<WorkingWeekInfoModel> WorkingWeek { get; set; }
        public WeekCalendarModel()
        {
            WorkingWeek = new List<WorkingWeekInfoModel>();
        }
    }
}
