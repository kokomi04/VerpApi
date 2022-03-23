using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OvertimeLevel
    {
        public OvertimeLevel()
        {
            TimeSheetOvertime = new HashSet<TimeSheetOvertime>();
        }

        public int OvertimeLevelId { get; set; }
        public int OrdinalNumber { get; set; }
        public decimal OvertimeRate { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<TimeSheetOvertime> TimeSheetOvertime { get; set; }
    }
}
