using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductBom
    {
        public long ProductBomId { get; set; }
        public int ProductId { get; set; }
        public int? ChildProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Wastage { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int? InputStepId { get; set; }
        public int? OutputStepId { get; set; }
        public int? SortOrder { get; set; }

        public virtual Product ChildProduct { get; set; }
        public virtual Product Product { get; set; }
    }
}
