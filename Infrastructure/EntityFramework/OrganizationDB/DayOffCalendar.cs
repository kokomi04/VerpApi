using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class DayOffCalendar
    {
        public int SubsidiaryId { get; set; }
        public DateTime Day { get; set; }
        public string Content { get; set; }
    }
}
