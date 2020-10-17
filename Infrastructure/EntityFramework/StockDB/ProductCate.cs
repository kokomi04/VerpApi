using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductCate
    {
        public ProductCate()
        {
            InverseParentProductCate = new HashSet<ProductCate>();
            Product = new HashSet<Product>();
        }

        public int ProductCateId { get; set; }
        public string ProductCateName { get; set; }
        public int? ParentProductCateId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int SortOrder { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ProductCate ParentProductCate { get; set; }
        public virtual ICollection<ProductCate> InverseParentProductCate { get; set; }
        public virtual ICollection<Product> Product { get; set; }
    }
}
