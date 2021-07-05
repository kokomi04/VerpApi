using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionProcessMold
    {
        public ProductionProcessMold()
        {
            ProductionStepMold = new HashSet<ProductionStepMold>();
        }

        public long ProductionProcessMoldId { get; set; }
        public string Title { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ICollection<ProductionStepMold> ProductionStepMold { get; set; }
    }
}
