using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class TimeSheet
    {
        public TimeSheet()
        {
            TimeSheetDetail = new HashSet<TimeSheetDetail>();
        }

        public long TimeSheetId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Note { get; set; }
        public bool IsApprove { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<TimeSheetDetail> TimeSheetDetail { get; set; }
    }
}
