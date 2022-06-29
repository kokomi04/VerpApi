using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderDetailSubCalculation
    {
        public int PurchaseOrderDetailSubCalculationId { get; set; }
        public long PurchaseOrderDetailId { get; set; }
        public long ProductBomId { get; set; }
        public int? UnitConversionId { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PurchaseOrderDetail PurchaseOrderDetail { get; set; }
    }
}
