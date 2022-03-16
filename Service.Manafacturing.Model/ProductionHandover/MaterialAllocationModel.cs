using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class MaterialAllocationModel : IMapFrom<MaterialAllocation>
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
        public long? InventoryDetailId { get; set; }
    }

    public class AllocationModel
    {
        public IList<MaterialAllocationModel> MaterialAllocations { get; set; }
        public IList<IgnoreAllocationModel> IgnoreAllocations { get; set; }
    }
}
