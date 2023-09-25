using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftSchedule
{
    public long ShiftScheduleId { get; set; }

    public string Title { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int OvertimeMode { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<ShiftScheduleConfiguration> ShiftScheduleConfiguration { get; set; } = new List<ShiftScheduleConfiguration>();

    public virtual ICollection<ShiftScheduleDepartment> ShiftScheduleDepartment { get; set; } = new List<ShiftScheduleDepartment>();

    public virtual ICollection<ShiftScheduleDetail> ShiftScheduleDetail { get; set; } = new List<ShiftScheduleDetail>();
}
