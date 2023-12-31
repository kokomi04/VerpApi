﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class OutsourceStepRequest
{
    public long OutsourceStepRequestId { get; set; }

    public string OutsourceStepRequestCode { get; set; }

    public long ProductionOrderId { get; set; }

    public DateTime OutsourceStepRequestFinishDate { get; set; }

    public int OutsourceStepRequestStatusId { get; set; }

    public bool IsInvalid { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public string Setting { get; set; }

    public virtual ICollection<OutsourceStepRequestData> OutsourceStepRequestData { get; set; } = new List<OutsourceStepRequestData>();

    public virtual ICollection<ProductionMaterialsRequirementDetail> ProductionMaterialsRequirementDetail { get; set; } = new List<ProductionMaterialsRequirementDetail>();

    public virtual ProductionOrder ProductionOrder { get; set; }

    public virtual ICollection<ProductionStep> ProductionStep { get; set; } = new List<ProductionStep>();
}
