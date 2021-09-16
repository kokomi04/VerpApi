using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingSuggestDetail
    {
        public PurchasingSuggestDetail()
        {
            PoAssignmentDetail = new HashSet<PoAssignmentDetail>();
            PurchaseOrderDetail = new HashSet<PurchaseOrderDetail>();
        }

        public long PurchasingSuggestDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public long PurchasingSuggestId { get; set; }
        public int CustomerId { get; set; }
        public long? PurchasingRequestDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public decimal? IntoMoney { get; set; }
        public int? SortOrder { get; set; }

        public virtual PurchasingRequestDetail PurchasingRequestDetail { get; set; }
        public virtual PurchasingSuggest PurchasingSuggest { get; set; }
        public virtual ICollection<PoAssignmentDetail> PoAssignmentDetail { get; set; }
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
    }
}
