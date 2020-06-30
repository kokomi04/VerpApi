using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderFile
    {
        public long PurchaseOrderId { get; set; }
        public long FileId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }
    }
}
