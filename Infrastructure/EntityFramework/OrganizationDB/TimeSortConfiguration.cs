using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSortConfiguration
{
    public int TimeSortConfigurationId { get; set; }

    public string TimeSortCode { get; set; }

    public string TimeSortDescription { get; set; }

    public int TimeSortType { get; set; }

    public long MinMinutes { get; set; }

    public long MaxMinutes { get; set; }

    public long BetweenMinutes { get; set; }

    public int NumberOfCycles { get; set; }

    public TimeSpan TimeEndCycles { get; set; }

    public bool IsIgnoreNightShift { get; set; }

    public TimeSpan StartTimeIgnoreTimeShift { get; set; }

    public TimeSpan EndTimeIgnoreTimeShift { get; set; }

    public bool IsApplySpecialCase { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<SplitHour> SplitHour { get; set; } = new List<SplitHour>();

    public virtual ICollection<WorkSchedule> WorkSchedule { get; set; } = new List<WorkSchedule>();
}
