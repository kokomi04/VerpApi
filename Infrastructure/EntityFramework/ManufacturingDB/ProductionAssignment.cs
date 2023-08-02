using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionAssignment
{
    public long ProductionStepId { get; set; }

    public int DepartmentId { get; set; }

    public decimal AssignmentQuantity { get; set; }

    public decimal? AssignmentWorkload { get; set; }

    public decimal? AssignmentHours { get; set; }

    public long ProductionStepLinkDataId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public long ProductionOrderId { get; set; }

    public int AssignedProgressStatus { get; set; }

    public bool IsManualFinish { get; set; }

    public bool IsManualSetDate { get; set; }

    public bool IsManualSetEndDate { get; set; }

    public bool IsManualSetStartDate { get; set; }

    public decimal RateInPercent { get; set; }

    public virtual ICollection<ProductionAssignmentDetail> ProductionAssignmentDetail { get; set; } = new List<ProductionAssignmentDetail>();

    public virtual ICollection<ProductionConsumMaterial> ProductionConsumMaterial { get; set; } = new List<ProductionConsumMaterial>();

    public virtual ProductionStepLinkData ProductionStepLinkData { get; set; }
}
