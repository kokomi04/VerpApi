using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class OvertimeLevel
{
    public int OvertimeLevelId { get; set; }

    public decimal OvertimeRate { get; set; }

    public string OvertimeCode { get; set; }

    public string Description { get; set; }

    public int OvertimePriority { get; set; }

    public int SortOrder { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<OvertimeConfigurationMapping> OvertimeConfigurationMapping { get; set; } = new List<OvertimeConfigurationMapping>();
}
