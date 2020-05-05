using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDetail
    {
        public long PurchaseOrderDetailId { get; set; }
        public long PurchaseOrderId { get; set; }
        public long? PoAssignmentDetailId { get; set; }
        public long? PurchasingSuggestDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProviderProductName { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? PrimaryUnitPrice { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PoAssignmentDetail PoAssignmentDetail { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        public virtual PurchasingSuggestDetail PurchasingSuggestDetail { get; set; }
    }
}
