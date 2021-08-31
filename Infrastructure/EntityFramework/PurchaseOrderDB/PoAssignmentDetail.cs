using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PoAssignmentDetail
    {
        public PoAssignmentDetail()
        {
            PurchaseOrderDetail = new HashSet<PurchaseOrderDetail>();
        }

        public long PoAssignmentDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public long PoAssignmentId { get; set; }
        public long PurchasingSuggestDetailId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PoAssignment PoAssignment { get; set; }
        public virtual PurchasingSuggestDetail PurchasingSuggestDetail { get; set; }
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
    }
}
