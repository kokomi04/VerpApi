using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class HrTypeGroup
{
    public int HrTypeGroupId { get; set; }

    public string HrTypeGroupName { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<HrType> HrType { get; set; } = new List<HrType>();
}
