﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class DepartmentCalendar
{
    public int CalendarId { get; set; }

    public int DepartmentId { get; set; }

    public DateTime StartDate { get; set; }
}
