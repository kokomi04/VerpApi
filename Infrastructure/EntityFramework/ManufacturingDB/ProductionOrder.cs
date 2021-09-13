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
            ProductionMaterialsRequirement = new HashSet<ProductionMaterialsRequirement>();
            ProductionOrderAttachment = new HashSet<ProductionOrderAttachment>();
            ProductionOrderDetail = new HashSet<ProductionOrderDetail>();
            ProductionOrderMaterials = new HashSet<ProductionOrderMaterials>();
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
        public DateTime PlanEndDate { get; set; }

        public virtual ICollection<OutsourceStepRequest> OutsourceStepRequest { get; set; }
        public virtual ICollection<ProductionMaterialsRequirement> ProductionMaterialsRequirement { get; set; }
        public virtual ICollection<ProductionOrderAttachment> ProductionOrderAttachment { get; set; }
        public virtual ICollection<ProductionOrderDetail> ProductionOrderDetail { get; set; }
        public virtual ICollection<ProductionOrderMaterials> ProductionOrderMaterials { get; set; }
    }
}
