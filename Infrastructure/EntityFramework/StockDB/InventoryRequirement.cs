using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryRequirement
    {
        public InventoryRequirement()
        {
            InventoryRequirementDetail = new HashSet<InventoryRequirementDetail>();
            InventoryRequirementFile = new HashSet<InventoryRequirementFile>();
        }

        public long InventoryRequirementId { get; set; }
        public int SubsidiaryId { get; set; }
        public string InventoryRequirementCode { get; set; }
        public int InventoryTypeId { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int? DepartmentId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public long? ProductionHandoverId { get; set; }

        public virtual ICollection<InventoryRequirementDetail> InventoryRequirementDetail { get; set; }
        public virtual ICollection<InventoryRequirementFile> InventoryRequirementFile { get; set; }
    }
}
