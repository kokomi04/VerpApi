using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ProductPriceConfigItemPrice
    {
        public long ProductPriceConfigItemPriceId { get; set; }
        public int SubsidiaryId { get; set; }
        public int ProductPriceConfigId { get; set; }
        public bool? IsForeignPrice { get; set; }
        public bool? IsEditable { get; set; }
        public string ItemKey { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductPriceConfig ProductPriceConfig { get; set; }
    }
}
