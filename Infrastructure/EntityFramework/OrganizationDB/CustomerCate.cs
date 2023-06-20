using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class CustomerCate
{
    public int CustomerCateId { get; set; }

    public string CustomerCateCode { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int? SortOrder { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<Customer> Customer { get; set; } = new List<Customer>();
}
