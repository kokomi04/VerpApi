using System;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PoProviderPricingDetail
    {
        public long PoProviderPricingDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public long PoProviderPricingId { get; set; }
        public int ProductId { get; set; }
        public string ProviderProductName { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public decimal? IntoMoney { get; set; }
        public decimal? ExchangedMoney { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int? SortOrder { get; set; }

        public virtual PoProviderPricing PoProviderPricing { get; set; }
    }
}
