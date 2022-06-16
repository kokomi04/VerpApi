using System.Collections.Generic;
using VErp.Services.Organization.Model.Calendar;

namespace VErp.Services.Organization.Model.DepartmentCalendar

{
    public class DepartmentCalendarListModel
    {
        public int DepartmentId { get; set; }
        public ICollection<WorkingHourInfoModel> DepartmentWorkingHourInfo { get; set; }
        public ICollection<DayOffCalendarModel> DepartmentDayOffCalendar { get; set; }
        public ICollection<DepartmentOverHourInfoModel> DepartmentOverHourInfo { get; set; }
        public ICollection<DepartmentIncreaseInfoModel> DepartmentIncreaseInfo { get; set; }
        public DepartmentCalendarListModel()
        {
            DepartmentWorkingHourInfo = new List<WorkingHourInfoModel>();
            DepartmentDayOffCalendar = new List<DayOffCalendarModel>();
            DepartmentOverHourInfo = new List<DepartmentOverHourInfoModel>();
            DepartmentIncreaseInfo = new List<DepartmentIncreaseInfoModel>();
        }
    }

    public class WorkingHourInfoModel
    {
        public double WorkingHourPerDay { get; set; }
        public long StartDate { get; set; }
    }
}
