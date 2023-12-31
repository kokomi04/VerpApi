﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class MaterialCalcSummary
{
    public long MaterialCalcSummaryId { get; set; }

    public long MaterialCalcId { get; set; }

    public int OriginalMaterialProductId { get; set; }

    public int MaterialProductId { get; set; }

    public decimal MaterialQuantity { get; set; }

    public decimal ExchangeRate { get; set; }

    public bool IsSubCalculation { get; set; }

    public string? OrdersQuantity { get; set; }

    public virtual MaterialCalc MaterialCalc { get; set; }

    public virtual ICollection<MaterialCalcSummarySubCalculation> MaterialCalcSummarySubCalculation { get; set; } = new List<MaterialCalcSummarySubCalculation>();
}
