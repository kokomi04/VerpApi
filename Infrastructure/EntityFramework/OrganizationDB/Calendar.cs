#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class Calendar
    {
        public int CalendarId { get; set; }
        public string CalendarCode { get; set; }
        public string CalendarName { get; set; }
        public string Guide { get; set; }
        public string Note { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
