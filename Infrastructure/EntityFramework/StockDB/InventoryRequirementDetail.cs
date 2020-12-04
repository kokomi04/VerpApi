using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryRequirementDetail
    {
        public long InventoryRequirementDetailId { get; set; }
        public int SubsidiaryId { get; set; }
        public long InventoryRequirementId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }
        public string Pocode { get; set; }
        public string ProductionOrderCode { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int? SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int? AssignStockId { get; set; }

        public virtual InventoryRequirement InventoryRequirement { get; set; }
        public virtual ProductUnitConversion ProductUnitConversion { get; set; }
    }
}
