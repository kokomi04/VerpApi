using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductProperty
    {
        public ProductProperty()
        {
            ProductMaterialProperty = new HashSet<ProductMaterialProperty>();
        }

        public int ProductPropertyId { get; set; }
        public string PropertyName { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<ProductMaterialProperty> ProductMaterialProperty { get; set; }
    }
}
