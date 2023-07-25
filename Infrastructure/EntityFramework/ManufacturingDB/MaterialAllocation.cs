//using System;
//using System.Collections.Generic;

//namespace VErp.Infrastructure.EF.ManufacturingDB;

//public partial class MaterialAllocation
//{
//    public long MaterialAllocationId { get; set; }

//    public long ProductionOrderId { get; set; }

//    public string InventoryCode { get; set; }

//    /// <summary>
//    /// Product id in inventory detail
//    /// </summary>
//    public int ProductId { get; set; }

//    public int DepartmentId { get; set; }

//    public long ProductionStepId { get; set; }

//    public decimal AllocationQuantity { get; set; }

//    /// <summary>
//    /// Product id in production process
//    /// </summary>
//    public int? SourceProductId { get; set; }

//    /// <summary>
//    /// Product quantity output in production process
//    /// </summary>
//    public decimal? SourceQuantity { get; set; }

//    public int SubsidiaryId { get; set; }

//    public long? InventoryDetailId { get; set; }
//}
