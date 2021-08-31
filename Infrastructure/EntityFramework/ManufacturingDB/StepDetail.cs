using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class StepDetail
    {
        public int StepDetailId { get; set; }
        public int StepId { get; set; }
        public int DepartmentId { get; set; }
        public decimal WorkingHours { get; set; }
        public int NumberOfPerson { get; set; }
        public decimal Quantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual Step Step { get; set; }
    }
}
