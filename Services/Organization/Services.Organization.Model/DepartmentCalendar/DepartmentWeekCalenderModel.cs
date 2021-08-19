using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Organization.Model.DepartmentCalendar

{
    public class DepartmentWeekCalendarModel
    {
        public int DepartmentId { get; set; }
        public double WorkingHourPerDay { get; set; }
        public ICollection<DepartmentWorkingWeekInfoModel> DepartmentWorkingWeek { get; set; }
        public DepartmentWeekCalendarModel()
        {
            DepartmentWorkingWeek = new List<DepartmentWorkingWeekInfoModel>();
        }
    }
}
