using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrderMaterialsConsumption
    {
        public long ProductionOrderMaterialsConsumptionId { get; set; }
        public long ProductionOrderId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public long ProductId { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal Quantity { get; set; }
        public int? DepartmentId { get; set; }
        public int InventoryRequirementStatusId { get; set; }
        public long? ParentId { get; set; }
        public bool IsReplacement { get; set; }
        public int UnitId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
