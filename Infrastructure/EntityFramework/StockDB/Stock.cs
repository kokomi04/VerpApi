using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Stock
    {
        public Stock()
        {
            Inventory = new HashSet<Inventory>();
            InventoryRequirementDetail = new HashSet<InventoryRequirementDetail>();
            Package = new HashSet<Package>();
        }

        public int StockId { get; set; }
        public int SubsidiaryId { get; set; }
        public string StockName { get; set; }
        public string Description { get; set; }
        public int? StockKeeperId { get; set; }
        public string StockKeeperName { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<Inventory> Inventory { get; set; }
        public virtual ICollection<InventoryRequirementDetail> InventoryRequirementDetail { get; set; }
        public virtual ICollection<Package> Package { get; set; }
    }
}
