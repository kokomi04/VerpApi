using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class DataConfig
{
    public int SubsidiaryId { get; set; }

    public DateTime ClosingDate { get; set; }

    public bool AutoClosingDate { get; set; }

    public string FreqClosingDate { get; set; }

    public DateTime? WorkingFromDate { get; set; }

    public DateTime? WorkingToDate { get; set; }

    public bool? IsIgnoreAccountant { get; set; }

    public bool? IsAutoUpdateWorkingDate { get; set; }
}
