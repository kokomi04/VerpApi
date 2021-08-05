using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ProductPriceInfo
    {
        public ProductPriceInfo()
        {
            ProductPriceInfoItem = new HashSet<ProductPriceInfoItem>();
        }

        public long ProductPriceInfoId { get; set; }
        public int? ProductId { get; set; }
        public int? ProductPriceConfigVersionId { get; set; }
        public string EvalData { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public decimal? FinalPrice { get; set; }

        public virtual ProductPriceConfigVersion ProductPriceConfigVersion { get; set; }
        public virtual ICollection<ProductPriceInfoItem> ProductPriceInfoItem { get; set; }
    }
}
