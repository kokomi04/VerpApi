using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class HrArea
{
    public int HrAreaId { get; set; }

    public int HrTypeId { get; set; }

    public string HrAreaCode { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public bool IsMultiRow { get; set; }

    public bool IsAddition { get; set; }

    public int Columns { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public string ColumnStyles { get; set; }

    public int? HrTypeReferenceId { get; set; }

    public virtual ICollection<HrAreaField> HrAreaField { get; set; } = new List<HrAreaField>();

    public virtual ICollection<HrField> HrField { get; set; } = new List<HrField>();

    public virtual HrType HrType { get; set; }
}
