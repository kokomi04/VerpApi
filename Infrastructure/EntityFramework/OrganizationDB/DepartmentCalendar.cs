using System;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class DepartmentCalendar
    {
        public int CalendarId { get; set; }
        public int DepartmentId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
