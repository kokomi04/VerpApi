﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class Step
{
    public int StepId { get; set; }

    public string StepName { get; set; }

    public int SortOrder { get; set; }

    public int StepGroupId { get; set; }

    public decimal ShrinkageRate { get; set; }

    public int HandoverTypeId { get; set; }

    public bool IsHide { get; set; }

    public string Description { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public virtual ICollection<ProductionStep> ProductionStep { get; set; } = new List<ProductionStep>();

    public virtual ICollection<ProductionStepMold> ProductionStepMold { get; set; } = new List<ProductionStepMold>();

    public virtual ICollection<StepDetail> StepDetail { get; set; } = new List<StepDetail>();

    public virtual StepGroup StepGroup { get; set; }
}
