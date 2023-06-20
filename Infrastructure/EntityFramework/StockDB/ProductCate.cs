using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class ProductCate
{
    public int ProductCateId { get; set; }

    public string ProductCateName { get; set; }

    public int? ParentProductCateId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public int SortOrder { get; set; }

    public int SubsidiaryId { get; set; }

    public bool IsDefault { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<ProductCate> InverseParentProductCate { get; set; } = new List<ProductCate>();

    public virtual ProductCate ParentProductCate { get; set; }

    public virtual ICollection<Product> Product { get; set; } = new List<Product>();
}
