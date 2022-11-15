﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MaterialCalcSummary
    {
        public MaterialCalcSummary()
        {
            MaterialCalcSummarySubCalculation = new HashSet<MaterialCalcSummarySubCalculation>();
        }

        public long MaterialCalcSummaryId { get; set; }
        public long MaterialCalcId { get; set; }
        public int OriginalMaterialProductId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }
        public decimal ExchangeRate { get; set; }

        public virtual MaterialCalc MaterialCalc { get; set; }
        public virtual ICollection<MaterialCalcSummarySubCalculation> MaterialCalcSummarySubCalculation { get; set; }
    }
}
