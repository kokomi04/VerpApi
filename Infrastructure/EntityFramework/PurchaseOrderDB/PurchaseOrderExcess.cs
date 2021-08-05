using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderExcess
    {
        public long PurchaseOrderExcessId { get; set; }
        public long PurchaseOrderId { get; set; }
        public string Title { get; set; }
        public int UnitId { get; set; }
        public string Specification { get; set; }
        public decimal Quantity { get; set; }
        public int? DecimalPlace { get; set; }
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
