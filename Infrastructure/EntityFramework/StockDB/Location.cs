using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Location
    {
        public Location()
        {
            Package = new HashSet<Package>();
        }

        public int LocationId { get; set; }
        public int StockId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDatetimeUtc { get; set; }
        public DateTime? UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<Package> Package { get; set; }
    }
}
