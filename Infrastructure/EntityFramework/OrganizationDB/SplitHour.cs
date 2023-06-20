using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SplitHour
{
    public int SplitHourId { get; set; }

    public int TimeSortConfigurationId { get; set; }

    public TimeSpan StartTimeOn { get; set; }

    public TimeSpan EndTimeOn { get; set; }

    public TimeSpan StartTimeOut { get; set; }

    public TimeSpan EndTimeOut { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual TimeSortConfiguration TimeSortConfiguration { get; set; }
}
