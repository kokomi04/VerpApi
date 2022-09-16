using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrderMaterialSet
    {
        public ProductionOrderMaterialSet()
        {
            ProductionOrderMaterialSetConsumptionGroup = new HashSet<ProductionOrderMaterialSetConsumptionGroup>();
            ProductionOrderMaterials = new HashSet<ProductionOrderMaterials>();
        }

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
        public virtual ICollection<ProductionOrderMaterialSetConsumptionGroup> ProductionOrderMaterialSetConsumptionGroup { get; set; }
        public virtual ICollection<ProductionOrderMaterials> ProductionOrderMaterials { get; set; }
    }
}
