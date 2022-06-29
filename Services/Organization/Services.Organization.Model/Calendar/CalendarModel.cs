using VErp.Commons.GlobalObject;
using CalendarEntity = VErp.Infrastructure.EF.OrganizationDB.Calendar;
namespace VErp.Services.Organization.Model.Calendar
{
    public class CalendarModel : IMapFrom<CalendarEntity>
    {
        public int CalendarId { get; set; }
        public string CalendarCode { get; set; }
        public string CalendarName { get; set; }
        public string Guide { get; set; }
        public string Note { get; set; }
    }
}