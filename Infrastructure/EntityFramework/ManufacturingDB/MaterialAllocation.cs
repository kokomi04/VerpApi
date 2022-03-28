using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class MaterialAllocation
    {
        public int MaterialAllocationId { get; set; }
        public long ProductionOrderId { get; set; }
        public string InventoryCode { get; set; }
        public int ProductId { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public decimal AllocationQuantity { get; set; }
        public int? SourceProductId { get; set; }
        public decimal? SourceQuantity { get; set; }
        public int SubsidiaryId { get; set; }
        public long? InventoryDetailId { get; set; }
    }
}
