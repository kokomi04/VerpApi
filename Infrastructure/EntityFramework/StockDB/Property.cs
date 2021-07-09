using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Property
    {
        public Property()
        {
            ProductProperty = new HashSet<ProductProperty>();
        }

        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<ProductProperty> ProductProperty { get; set; }
    }
}
