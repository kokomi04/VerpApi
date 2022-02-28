using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class WorkScheduleMark
    {
        public int WorkScheduleMarkId { get; set; }
        public int EmployeeId { get; set; }
        public int WorkScheduleId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual WorkSchedule WorkSchedule { get; set; }
    }
}
