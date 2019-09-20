using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductIdentityCode
    {
        public ProductIdentityCode()
        {
            InverseParentProductIdentityCode = new HashSet<ProductIdentityCode>();
            Product = new HashSet<Product>();
        }

        public int ProductIdentityCodeId { get; set; }
        public string ProductIdentityCodeName { get; set; }
        public int? ParentProductIdentityCodeId { get; set; }
        public string IdentityCode { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ProductIdentityCode ParentProductIdentityCode { get; set; }
        public virtual ICollection<ProductIdentityCode> InverseParentProductIdentityCode { get; set; }
        public virtual ICollection<Product> Product { get; set; }
    }
}
