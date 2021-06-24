using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepMold
    {
        public ProductionStepMold()
        {
            ProductionStepMoldLinkFromProductionStepMold = new HashSet<ProductionStepMoldLink>();
            ProductionStepMoldLinkToProductionStepMold = new HashSet<ProductionStepMoldLink>();
        }

        public long ProductionStepMoldId { get; set; }
        public long ProductionProcessMoldId { get; set; }
        public int StepId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ProductionProcessMold ProductionProcessMold { get; set; }
        public virtual Step Step { get; set; }
        public virtual ICollection<ProductionStepMoldLink> ProductionStepMoldLinkFromProductionStepMold { get; set; }
        public virtual ICollection<ProductionStepMoldLink> ProductionStepMoldLinkToProductionStepMold { get; set; }
    }
}
