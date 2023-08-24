using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionOrder
{
    public long ProductionOrderId { get; set; }

    public string ProductionOrderCode { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Description { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public bool IsDraft { get; set; }

    public int SubsidiaryId { get; set; }

    public int ProductionOrderStatus { get; set; }

    public DateTime Date { get; set; }

    public bool IsInvalid { get; set; }

    public bool IsResetProductionProcess { get; set; }

    public bool? InvalidPlan { get; set; }

    public DateTime? PlanEndDate { get; set; }

    public bool? IsUpdateQuantity { get; set; }

    public string ProductionOrderProcessVersion { get; set; }

    public bool? IsUpdateProcessForAssignment { get; set; }

    public int? MonthPlanId { get; set; }

    public int? FromWeekPlanId { get; set; }

    public int? ToWeekPlanId { get; set; }

    public int? FactoryDepartmentId { get; set; }

    public int? ProductionOrderAssignmentStatusId { get; set; }

    public bool IsManualFinish { get; set; }

    public bool IsFinished { get; set; }

    public virtual WeekPlan FromWeekPlan { get; set; }

    public virtual MonthPlan MonthPlan { get; set; }

    public virtual ICollection<OutsourceStepRequest> OutsourceStepRequest { get; set; } = new List<OutsourceStepRequest>();

    public virtual ICollection<ProductionHandover> ProductionHandover { get; set; } = new List<ProductionHandover>();

    public virtual ICollection<ProductionHumanResource> ProductionHumanResource { get; set; } = new List<ProductionHumanResource>();

    public virtual ICollection<ProductionMaterialsRequirement> ProductionMaterialsRequirement { get; set; } = new List<ProductionMaterialsRequirement>();

    public virtual ICollection<ProductionOrderAttachment> ProductionOrderAttachment { get; set; } = new List<ProductionOrderAttachment>();

    public virtual ICollection<ProductionOrderDetail> ProductionOrderDetail { get; set; } = new List<ProductionOrderDetail>();

    public virtual ICollection<ProductionOrderMaterialSet> ProductionOrderMaterialSet { get; set; } = new List<ProductionOrderMaterialSet>();

    public virtual ICollection<ProductionOrderMaterials> ProductionOrderMaterials { get; set; } = new List<ProductionOrderMaterials>();

    public virtual ICollection<ProductionOrderMaterialsConsumption> ProductionOrderMaterialsConsumption { get; set; } = new List<ProductionOrderMaterialsConsumption>();

    public virtual WeekPlan ToWeekPlan { get; set; }
}
