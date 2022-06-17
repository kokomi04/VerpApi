using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryDetailChange
    {
        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int StockId { get; set; }
        public decimal OldPrimaryQuantity { get; set; }
        public decimal OldPuConversionQuantity { get; set; }
        public int ProductUnitConversionId { get; set; }
        public int ProductId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
