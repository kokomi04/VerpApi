using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public string ProductTypeName { get; set; }

    public int? ParentProductTypeId { get; set; }

    public string IdentityCode { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public int SortOrder { get; set; }

    public int SubsidiaryId { get; set; }

    public bool IsDefault { get; set; }

    public virtual ICollection<ProductType> InverseParentProductType { get; set; } = new List<ProductType>();

    public virtual ProductType ParentProductType { get; set; }

    public virtual ICollection<Product> Product { get; set; } = new List<Product>();
}
