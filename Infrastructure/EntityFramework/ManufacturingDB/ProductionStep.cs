using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStep
    {
        public ProductionStep()
        {
            InOutStepLink = new HashSet<InOutStepLink>();
        }

        public int ProductionStepId { get; set; }
        public int StepId { get; set; }
        public string Title { get; set; }
        public int? ParentId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SortOrder { get; set; }
        public bool? IsGroup { get; set; }
        public int UsingType { get; set; }

        public virtual Step Step { get; set; }
        public virtual ICollection<InOutStepLink> InOutStepLink { get; set; }
    }
}
