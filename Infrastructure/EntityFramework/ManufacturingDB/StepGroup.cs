﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class StepGroup
{
    public int StepGroupId { get; set; }

    public string StepGroupName { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public virtual ICollection<Step> Step { get; set; } = new List<Step>();
}
