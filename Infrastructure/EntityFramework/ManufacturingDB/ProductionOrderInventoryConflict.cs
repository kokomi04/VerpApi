using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionOrderInventoryConflict
{
    public long ProductionOrderId { get; set; }

    public long InventoryDetailId { get; set; }

    public int ProductId { get; set; }

    public int InventoryTypeId { get; set; }

    public long InventoryId { get; set; }

    public DateTime InventoryDate { get; set; }

    public string InventoryCode { get; set; }

    public decimal InventoryQuantity { get; set; }

    public long? InventoryRequirementDetailId { get; set; }

    public long? InventoryRequirementId { get; set; }

    public decimal? RequireQuantity { get; set; }

    public string InventoryRequirementCode { get; set; }

    public string Content { get; set; }

    public decimal HandoverInventoryQuantitySum { get; set; }

    public int ConflictAllowcationStatusId { get; set; }
}
