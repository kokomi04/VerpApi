using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrderMaterials
    {
        public ProductionOrderMaterials()
        {
            InverseParent = new HashSet<ProductionOrderMaterials>();
        }

        public long ProductionOrderMaterialsId { get; set; }
        public long ProductionOrderId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public int InventoryRequirementStatusId { get; set; }
        public long? ParentId { get; set; }
        public bool IsReplacement { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductionOrderMaterials Parent { get; set; }
        public virtual ProductionOrder ProductionOrder { get; set; }
        public virtual ICollection<ProductionOrderMaterials> InverseParent { get; set; }
    }
}
