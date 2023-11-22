using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class WorkingDate
{
    public int WorkingDateId { get; set; }

    public int UserId { get; set; }

    public int SubsidiaryId { get; set; }

    public DateTime? WorkingFromDate { get; set; }

    public DateTime? WorkingToDate { get; set; }

    public string WorkingDateConfig { get; set; }
}
