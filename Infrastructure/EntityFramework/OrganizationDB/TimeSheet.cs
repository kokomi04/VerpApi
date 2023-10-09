using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheet
{
    public long TimeSheetId { get; set; }

    public string Title { get; set; }

    public DateTime BeginDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Note { get; set; }

    public bool IsApprove { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<TimeSheetAggregate> TimeSheetAggregate { get; set; } = new List<TimeSheetAggregate>();

    public virtual ICollection<TimeSheetDepartment> TimeSheetDepartment { get; set; } = new List<TimeSheetDepartment>();

    public virtual ICollection<TimeSheetDetail> TimeSheetDetail { get; set; } = new List<TimeSheetDetail>();
}
