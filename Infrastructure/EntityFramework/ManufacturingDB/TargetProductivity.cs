using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class TargetProductivity
{
    public int TargetProductivityId { get; set; }

    public string TargetProductivityCode { get; set; }

    public DateTime TargetProductivityDate { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public bool IsDefault { get; set; }

    public string Note { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public decimal? EstimateProductionDays { get; set; }

    public decimal? EstimateProductionQuantity { get; set; }

    public virtual ICollection<TargetProductivityDetail> TargetProductivityDetail { get; set; } = new List<TargetProductivityDetail>();
}
