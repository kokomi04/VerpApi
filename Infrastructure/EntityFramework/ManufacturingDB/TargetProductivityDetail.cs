using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class TargetProductivityDetail
{
    public int TargetProductivityDetailId { get; set; }

    public int TargetProductivityId { get; set; }

    public decimal TargetProductivity { get; set; }

    public int ProductionStepId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int ProductivityTimeTypeId { get; set; }

    public int ProductivityResourceTypeId { get; set; }

    public string Note { get; set; }

    /// <summary>
    /// Option tính KLCV tính năng suất theo KL Tinh hay theo số lượng
    /// </summary>
    public int WorkLoadTypeId { get; set; }

    public decimal MinAssignHours { get; set; }

    public virtual TargetProductivity TargetProductivityNavigation { get; set; }
}
