using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Package
    {
        public Package()
        {
            InventoryDetail = new HashSet<InventoryDetail>();
        }

        public long PackageId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }

        public virtual Location Location { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetail { get; set; }
    }
}
