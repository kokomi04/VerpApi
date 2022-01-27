using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class LeaveConfig
    {
        public LeaveConfig()
        {
            Employee = new HashSet<Employee>();
            Leave = new HashSet<Leave>();
            LeaveConfigRole = new HashSet<LeaveConfigRole>();
            LeaveConfigValidation = new HashSet<LeaveConfigValidation>();
        }

        public int LeaveConfigId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int AdvanceDays { get; set; }
        public decimal? MonthRate { get; set; }
        public int? MaxAyear { get; set; }
        public int? SeniorityMonthsStart { get; set; }
        public int? SeniorityMonthOfYear { get; set; }
        public int? OldYearTransferMax { get; set; }
        public DateTime? OldYearAppliedToDate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<Employee> Employee { get; set; }
        public virtual ICollection<Leave> Leave { get; set; }
        public virtual ICollection<LeaveConfigRole> LeaveConfigRole { get; set; }
        public virtual ICollection<LeaveConfigValidation> LeaveConfigValidation { get; set; }
    }
}
