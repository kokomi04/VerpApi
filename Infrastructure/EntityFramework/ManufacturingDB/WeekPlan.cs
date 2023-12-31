﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class WeekPlan
{
    public int WeekPlanId { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int MonthPlanId { get; set; }

    public string WeekPlanName { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string WeekNote { get; set; }

    public virtual ICollection<ProductionOrder> ProductionOrderFromWeekPlan { get; set; } = new List<ProductionOrder>();

    public virtual ICollection<ProductionOrder> ProductionOrderToWeekPlan { get; set; } = new List<ProductionOrder>();
}
