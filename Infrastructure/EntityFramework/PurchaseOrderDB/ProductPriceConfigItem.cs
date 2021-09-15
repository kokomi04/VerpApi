using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ProductPriceConfigItem
    {
        public int ProductPriceConfigItemId { get; set; }
        public int ProductPriceConfigVersionId { get; set; }
        public string ItemKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsTable { get; set; }
        public string TableConfig { get; set; }
        public bool? IsEditable { get; set; }
        public bool IsPricing { get; set; }
        public bool IsForeignPrice { get; set; }
        public string OnChange { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductPriceConfigVersion ProductPriceConfigVersion { get; set; }
    }
}
