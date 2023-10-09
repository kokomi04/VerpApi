using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class MaterialCalc
{
    public long MaterialCalcId { get; set; }

    public int SubsidiaryId { get; set; }

    public string MaterialCalcCode { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public int? InputTypeSelectedState { get; set; }

    public int? InputUnitTypeSelectedState { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public long? PurchasingSuggestId { get; set; }

    public virtual ICollection<MaterialCalcConsumptionGroup> MaterialCalcConsumptionGroup { get; set; } = new List<MaterialCalcConsumptionGroup>();

    public virtual ICollection<MaterialCalcProduct> MaterialCalcProduct { get; set; } = new List<MaterialCalcProduct>();

    public virtual ICollection<MaterialCalcSummary> MaterialCalcSummary { get; set; } = new List<MaterialCalcSummary>();

    public virtual ICollection<PurchasingRequest> PurchasingRequest { get; set; } = new List<PurchasingRequest>();
}
