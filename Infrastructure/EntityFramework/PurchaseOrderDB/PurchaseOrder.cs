﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class PurchaseOrder
{
    public long PurchaseOrderId { get; set; }

    public int SubsidiaryId { get; set; }

    public string PurchaseOrderCode { get; set; }

    public int CustomerId { get; set; }

    public DateTime? Date { get; set; }

    public string PaymentInfo { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int? DeliveryUserId { get; set; }

    public int? DeliveryCustomerId { get; set; }

    public string DeliveryDestination { get; set; }

    public string Content { get; set; }

    public string AdditionNote { get; set; }

    public int PurchaseOrderStatusId { get; set; }

    public int? PoProcessStatusId { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal OtherFee { get; set; }

    public decimal TotalMoney { get; set; }

    public bool? IsApproved { get; set; }

    public bool? IsChecked { get; set; }

    public string PoDescription { get; set; }

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

    public int PurchaseOrderType { get; set; }

    public long? PropertyCalcId { get; set; }

    public decimal? TaxInPercent { get; set; }

    public decimal? ExchangeRate { get; set; }

    public long? CurrencyId { get; set; }

    public decimal? TaxInMoney { get; set; }

    public string DeliveryMethod { get; set; }

    public string PaymentMethod { get; set; }

    public string AttachmentBill { get; set; }

    public string Requirement { get; set; }

    public string DeliveryPolicy { get; set; }

    public string OtherPolicy { get; set; }

    public int? InputTypeSelectedState { get; set; }

    public int? InputUnitTypeSelectedState { get; set; }

    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetail { get; set; } = new List<PurchaseOrderDetail>();

    public virtual ICollection<PurchaseOrderExcess> PurchaseOrderExcess { get; set; } = new List<PurchaseOrderExcess>();

    public virtual ICollection<PurchaseOrderFile> PurchaseOrderFile { get; set; } = new List<PurchaseOrderFile>();

    public virtual ICollection<PurchaseOrderMaterials> PurchaseOrderMaterials { get; set; } = new List<PurchaseOrderMaterials>();

    public virtual ICollection<PurchaseOrderTracked> PurchaseOrderTracked { get; set; } = new List<PurchaseOrderTracked>();
}
