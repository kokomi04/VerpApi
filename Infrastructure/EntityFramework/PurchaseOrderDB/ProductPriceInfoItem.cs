using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ProductPriceInfoItem
    {
        public long ProductPriceInfoItemId { get; set; }
        public long ProductPriceInfoId { get; set; }
        public int ProductPriceConfigItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public string TableConfig { get; set; }
        public bool? IsEditable { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductPriceConfigItem ProductPriceConfigItem { get; set; }
        public virtual ProductPriceInfo ProductPriceInfo { get; set; }
    }
}
