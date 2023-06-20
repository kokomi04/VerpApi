using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrder
    {
        public ProductionOrder()
        {
            OutsourceStepRequest = new HashSet<OutsourceStepRequest>();
            ProductionHandover = new HashSet<ProductionHandover>();
            ProductionHumanResource = new HashSet<ProductionHumanResource>();
            ProductionMaterialsRequirement = new HashSet<ProductionMaterialsRequirement>();
            ProductionOrderAttachment = new HashSet<ProductionOrderAttachment>();
            ProductionOrderDetail = new HashSet<ProductionOrderDetail>();
            ProductionOrderMaterialSet = new HashSet<ProductionOrderMaterialSet>();
            ProductionOrderMaterials = new HashSet<ProductionOrderMaterials>();
            ProductionOrderMaterialsConsumption = new HashSet<ProductionOrderMaterialsConsumption>();
        }

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

        public virtual WeekPlan FromWeekPlan { get; set; }
        public virtual MonthPlan MonthPlan { get; set; }
        public virtual WeekPlan ToWeekPlan { get; set; }
        public virtual ICollection<OutsourceStepRequest> OutsourceStepRequest { get; set; }
        public virtual ICollection<ProductionHandover> ProductionHandover { get; set; }
        public virtual ICollection<ProductionHumanResource> ProductionHumanResource { get; set; }
        public virtual ICollection<ProductionMaterialsRequirement> ProductionMaterialsRequirement { get; set; }
        public virtual ICollection<ProductionOrderAttachment> ProductionOrderAttachment { get; set; }
        public virtual ICollection<ProductionOrderDetail> ProductionOrderDetail { get; set; }
        public virtual ICollection<ProductionOrderMaterialSet> ProductionOrderMaterialSet { get; set; }
        public virtual ICollection<ProductionOrderMaterials> ProductionOrderMaterials { get; set; }
        public virtual ICollection<ProductionOrderMaterialsConsumption> ProductionOrderMaterialsConsumption { get; set; }
    }
}
