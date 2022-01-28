using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.DepartmentCalendar

{
    public class DepartmentCalendarModel
    {
        public int CalendarId { get; set; }
        public string CalendarCode { get; set; }
        public string CalendarName { get; set; }
        public long? StartDate { get; set; }
    }


}
