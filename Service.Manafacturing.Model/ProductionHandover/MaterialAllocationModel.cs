using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class MaterialAllocationModel
    {
        public long MaterialAllocationId { get; set; }
        public long ProductionOrderId { get; set; }
        public long InventoryId { get; set; }
        public string InventoryCode { get; set; }
        public int ProductId { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public decimal AllocationQuantity { get; set; }
        public int? SourceProductId { get; set; }
        public decimal? SourceQuantity { get; set; }
        public long? InventoryDetailId { get; set; }
    }

    public class AllocationModel
    {
        public IList<MaterialAllocationModel> MaterialAllocations { get; set; }
        public IList<IgnoreAllocationModel> IgnoreAllocations { get; set; }
    }
}
