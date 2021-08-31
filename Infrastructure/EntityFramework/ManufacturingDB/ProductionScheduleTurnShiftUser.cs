using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionScheduleTurnShiftUser
    {
        public long ProductionScheduleTurnShiftUserId { get; set; }
        public long ProductionScheduleTurnShiftId { get; set; }
        public int UserId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Money { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ProductionScheduleTurnShift ProductionScheduleTurnShift { get; set; }
    }
}
