using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingSuggestDetailSubCalculation
    {
        public int PurchasingSuggestDetailSubCalculationId { get; set; }
        public long PurchasingSuggestDetailId { get; set; }
        public long ProductBomId { get; set; }
        public int? UnitConversionId { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PurchasingSuggestDetail PurchasingSuggestDetail { get; set; }
    }
}
