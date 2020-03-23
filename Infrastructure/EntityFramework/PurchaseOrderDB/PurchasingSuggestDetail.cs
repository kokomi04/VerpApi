using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingSuggestDetail
    {
        public PurchasingSuggestDetail()
        {
            PoAssignmentDetail = new HashSet<PoAssignmentDetail>();
        }

        public long PurchasingSuggestDetailId { get; set; }
        public long PurchasingSuggestId { get; set; }
        public int CustomerId { get; set; }
        public string PurchasingRequestIds { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? PrimaryUnitPrice { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PurchasingSuggest PurchasingSuggest { get; set; }
        public virtual ICollection<PoAssignmentDetail> PoAssignmentDetail { get; set; }
    }
}
