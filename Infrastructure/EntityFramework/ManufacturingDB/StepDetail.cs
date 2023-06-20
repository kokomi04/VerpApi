using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class StepDetail
{
    public int StepDetailId { get; set; }

    public int StepId { get; set; }

    public int DepartmentId { get; set; }

    /// <summary>
    /// Nang suat/nguoi-may
    /// </summary>
    public decimal QuantityBak { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int SubsidiaryId { get; set; }

    public int EstimateHandoverTime { get; set; }

    public virtual Step Step { get; set; }
}
