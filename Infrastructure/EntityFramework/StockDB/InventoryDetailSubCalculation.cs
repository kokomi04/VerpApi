using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryDetailSubCalculation
    {
        public int InventoryDetailSubCalculationId { get; set; }
        public long InventoryDetailId { get; set; }
        public long ProductBomId { get; set; }
        public int UnitConversionId { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }

        public virtual InventoryDetail InventoryDetail { get; set; }
    }
}
