using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingRequestDetail
    {
        public long PurchasingRequestDetailId { get; set; }
        public long PurchasingRequestId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PurchasingRequest PurchasingRequest { get; set; }
    }
}
