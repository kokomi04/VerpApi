﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class WorkingHourInfo
{
    public DateTime StartDate { get; set; }

    public int SubsidiaryId { get; set; }

    public double WorkingHourPerDay { get; set; }

    public int CalendarId { get; set; }
}
