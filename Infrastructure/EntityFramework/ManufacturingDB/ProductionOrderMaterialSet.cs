using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionOrderMaterialSet
{
    public long ProductionOrderMaterialSetId { get; set; }

    public string Title { get; set; }

    public long ProductionOrderId { get; set; }

    public bool IsMultipleConsumptionGroupId { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ProductionOrder ProductionOrder { get; set; }

    public virtual ICollection<ProductionOrderMaterialSetConsumptionGroup> ProductionOrderMaterialSetConsumptionGroup { get; set; } = new List<ProductionOrderMaterialSetConsumptionGroup>();

    public virtual ICollection<ProductionOrderMaterials> ProductionOrderMaterials { get; set; } = new List<ProductionOrderMaterials>();
}
