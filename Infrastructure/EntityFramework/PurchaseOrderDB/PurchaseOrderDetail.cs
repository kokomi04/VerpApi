using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDetail
    {
        public long PurchaseOrderDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public long PurchaseOrderId { get; set; }
        public long? PoAssignmentDetailId { get; set; }
        public long? PurchasingSuggestDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProviderProductName { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public long? OutsourceRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }
        public decimal? IntoMoney { get; set; }
        public decimal? IntoAfterTaxMoney { get; set; }

        public virtual PoAssignmentDetail PoAssignmentDetail { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        public virtual PurchasingSuggestDetail PurchasingSuggestDetail { get; set; }
    }
}
