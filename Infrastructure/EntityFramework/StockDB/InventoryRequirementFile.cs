using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryRequirementFile
    {
        public long InventoryRequirementId { get; set; }
        public long FileId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual InventoryRequirement InventoryRequirement { get; set; }
    }
}
