using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class TargetProductivityDetail
    {
        public int TargetProductivityDetailId { get; set; }
        public int TargetProductivityId { get; set; }
        public decimal TargetProductivity { get; set; }
        public int ProductionStepId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }

        public virtual TargetProductivity TargetProductivityNavigation { get; set; }
    }
}
