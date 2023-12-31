﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionMaterialsRequirementDetail
{
    public long ProductionMaterialsRequirementDetailId { get; set; }

    public long ProductionMaterialsRequirementId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public int DepartmentId { get; set; }

    public long ProductionStepId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public long? OutsourceStepRequestId { get; set; }

    public virtual OutsourceStepRequest OutsourceStepRequest { get; set; }

    public virtual ProductionMaterialsRequirement ProductionMaterialsRequirement { get; set; }

    public virtual ProductionStep ProductionStep { get; set; }
}
