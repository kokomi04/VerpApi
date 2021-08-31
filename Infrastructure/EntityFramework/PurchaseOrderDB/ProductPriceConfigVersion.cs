using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ProductPriceConfigVersion
    {
        public ProductPriceConfigVersion()
        {
            ProductPriceConfigItem = new HashSet<ProductPriceConfigItem>();
            ProductPriceInfo = new HashSet<ProductPriceInfo>();
        }

        public int ProductPriceConfigVersionId { get; set; }
        public int ProductPriceConfigId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EvalSourceCodeJs { get; set; }
        public string Fields { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductPriceConfig ProductPriceConfig { get; set; }
        public virtual ICollection<ProductPriceConfigItem> ProductPriceConfigItem { get; set; }
        public virtual ICollection<ProductPriceInfo> ProductPriceInfo { get; set; }
    }
}
