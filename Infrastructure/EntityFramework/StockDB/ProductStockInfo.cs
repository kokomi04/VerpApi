using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductStockInfo
    {
        public int ProductId { get; set; }
        public int? StockOutputRuleId { get; set; }
        public long? AmountWarningMin { get; set; }
        public long? AmountWarningMax { get; set; }
        public double? TimeWarningAmount { get; set; }
        public int? TimeWarningTimeTypeId { get; set; }
        public string DescriptionToStock { get; set; }
        public bool IsDeleted { get; set; }
        public int? ExpireTimeTypeId { get; set; }
        public double? ExpireTimeAmount { get; set; }

        public virtual Product Product { get; set; }
    }
}
