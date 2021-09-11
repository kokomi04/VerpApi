using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class MonthPlan
    {
        public int MonthPlanId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string MonthPlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MonthNote { get; set; }
    }
}
