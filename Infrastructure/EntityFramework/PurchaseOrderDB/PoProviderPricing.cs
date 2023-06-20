using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class PoProviderPricing
{
    public long PoProviderPricingId { get; set; }

    public int SubsidiaryId { get; set; }

    public string PoProviderPricingCode { get; set; }

    public int CustomerId { get; set; }

    public DateTime? Date { get; set; }

    public string PaymentInfo { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int? DeliveryUserId { get; set; }

    public int? DeliveryCustomerId { get; set; }

    public string DeliveryDestination { get; set; }

    public string Content { get; set; }

    public string AdditionNote { get; set; }

    public int PoProviderPricingStatusId { get; set; }

    public int? PoProcessStatusId { get; set; }

    public long? CurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public decimal? TaxInPercent { get; set; }

    public decimal? TaxInMoney { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal OtherFee { get; set; }

    public decimal TotalMoney { get; set; }

    public bool? IsApproved { get; set; }

    public bool? IsChecked { get; set; }

    public string PoProviderPricingDescription { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public int? CheckedByUserId { get; set; }

    public int? CensorByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public DateTime? CheckedDatetimeUtc { get; set; }

    public DateTime? CensorDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<PoProviderPricingDetail> PoProviderPricingDetail { get; set; } = new List<PoProviderPricingDetail>();

    public virtual ICollection<PoProviderPricingFile> PoProviderPricingFile { get; set; } = new List<PoProviderPricingFile>();
}
