using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class PropertyCalc
{
    public long PropertyCalcId { get; set; }

    public int SubsidiaryId { get; set; }

    public string PropertyCalcCode { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<CuttingWorkSheet> CuttingWorkSheet { get; set; } = new List<CuttingWorkSheet>();

    public virtual ICollection<PropertyCalcProduct> PropertyCalcProduct { get; set; } = new List<PropertyCalcProduct>();

    public virtual ICollection<PropertyCalcProperty> PropertyCalcProperty { get; set; } = new List<PropertyCalcProperty>();

    public virtual ICollection<PropertyCalcSummary> PropertyCalcSummary { get; set; } = new List<PropertyCalcSummary>();

    public virtual ICollection<PurchasingRequest> PurchasingRequest { get; set; } = new List<PurchasingRequest>();
}
