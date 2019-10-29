using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Inventory
    {
        public Inventory()
        {
            InventoryDetail = new HashSet<InventoryDetail>();
        }

        public long InventoryId { get; set; }
        public string InventoryCode { get; set; }
        public int InventoryTypeId { get; set; }
        public string Shipper { get; set; }
        public string Content { get; set; }
        public DateTime DateUtc { get; set; }
        public int? CustomerId { get; set; }
        public string Department { get; set; }
        public int? UserId { get; set; }
        public long? InvoiceFileId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<InventoryDetail> InventoryDetail { get; set; }
    }
}
