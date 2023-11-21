﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionAssignmentDetail
{
    public long ProductionStepId { get; set; }

    public int DepartmentId { get; set; }

    public DateTime WorkDate { get; set; }

    public decimal? QuantityPerDay { get; set; }

    public decimal? WorkloadPerDay { get; set; }

    public decimal? HoursPerDay { get; set; }

    public long ProductionOrderId { get; set; }

    public decimal? MinAssignHours { get; set; }

    public bool IsUseMinAssignHours { get; set; }

    public virtual ProductionAssignment ProductionAssignment { get; set; }
}
