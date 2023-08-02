using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class HrTypeView
{
    public int HrTypeViewId { get; set; }

    public string HrTypeViewName { get; set; }

    public int HrTypeId { get; set; }

    public int? UserId { get; set; }

    public bool IsDefault { get; set; }

    public int? Columns { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual HrType HrType { get; set; }

    public virtual ICollection<HrTypeViewField> HrTypeViewField { get; set; } = new List<HrTypeViewField>();
}
