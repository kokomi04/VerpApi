using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductMaterialsConsumptionGroup
    {
        public ProductMaterialsConsumptionGroup()
        {
            ProductMaterialsConsumption = new HashSet<ProductMaterialsConsumption>();
        }

        public int ProductMaterialsConsumptionGroupId { get; set; }
        public string ProductMaterialsConsumptionGroupCode { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<ProductMaterialsConsumption> ProductMaterialsConsumption { get; set; }
    }
}
