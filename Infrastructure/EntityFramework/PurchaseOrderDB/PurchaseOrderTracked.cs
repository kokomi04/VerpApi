using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderTracked
    {
        public long PurchaseOrderTrackedId { get; set; }
        public long PurchaseOrderId { get; set; }
        public DateTime Date { get; set; }
        public long? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }
    }
}
