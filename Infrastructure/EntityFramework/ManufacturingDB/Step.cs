using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class Step
    {
        public Step()
        {
            ProductionStep = new HashSet<ProductionStep>();
            ProductionStepMold = new HashSet<ProductionStepMold>();
            StepDetail = new HashSet<StepDetail>();
        }

        public int StepId { get; set; }
        public string StepName { get; set; }
        public int SortOrder { get; set; }
        public int StepGroupId { get; set; }
        public int UnitId { get; set; }
        public decimal ShrinkageRate { get; set; }
        public int HandoverTypeId { get; set; }
        public bool IsHide { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual StepGroup StepGroup { get; set; }
        public virtual ICollection<ProductionStep> ProductionStep { get; set; }
        public virtual ICollection<ProductionStepMold> ProductionStepMold { get; set; }
        public virtual ICollection<StepDetail> StepDetail { get; set; }
    }
}
