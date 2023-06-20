using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class MaterialCalcSummarySubCalculation
{
    public long MaterialCalcSummarySubCalculationId { get; set; }

    public long MaterialCalcSummaryId { get; set; }

    public long ProductBomId { get; set; }

    public decimal PrimaryQuantity { get; set; }

    public virtual MaterialCalcSummary MaterialCalcSummary { get; set; }
}
