using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class Property
{
    public int PropertyId { get; set; }

    public string PropertyCode { get; set; }

    public string PropertyName { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public string PropertyGroup { get; set; }

    public virtual ICollection<ProductProperty> ProductProperty { get; set; } = new List<ProductProperty>();
}
