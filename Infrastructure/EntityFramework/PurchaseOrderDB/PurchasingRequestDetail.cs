using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingRequestDetail
    {
        public PurchasingRequestDetail()
        {
            PurchasingSuggestDetail = new HashSet<PurchasingSuggestDetail>();
        }

        public long PurchasingRequestDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public long PurchasingRequestId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int? OriginalProductId { get; set; }
        public int? SortOrder { get; set; }

        public virtual PurchasingRequest PurchasingRequest { get; set; }
        public virtual ICollection<PurchasingSuggestDetail> PurchasingSuggestDetail { get; set; }
    }
}
