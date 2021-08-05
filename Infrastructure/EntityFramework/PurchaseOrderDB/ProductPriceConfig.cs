using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ProductPriceConfig
    {
        public ProductPriceConfig()
        {
            ProductPriceConfigVersion = new HashSet<ProductPriceConfigVersion>();
        }

        public int ProductPriceConfigId { get; set; }
        public int? SubsidiaryId { get; set; }
        public int LastestProductPriceConfigVersionId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<ProductPriceConfigVersion> ProductPriceConfigVersion { get; set; }
    }
}
