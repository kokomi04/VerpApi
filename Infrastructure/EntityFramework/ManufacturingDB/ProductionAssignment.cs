using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionAssignment
    {
        public ProductionAssignment()
        {
            ProductionAssignmentDetail = new HashSet<ProductionAssignmentDetail>();
            ProductionConsumMaterial = new HashSet<ProductionConsumMaterial>();
            ProductionHandoverProductionAssignment = new HashSet<ProductionHandover>();
            ProductionHandoverProductionAssignmentNavigation = new HashSet<ProductionHandover>();
            ProductionScheduleTurnShift = new HashSet<ProductionScheduleTurnShift>();
        }

        public long ProductionStepId { get; set; }
        public int DepartmentId { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal Productivity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public long ProductionOrderId { get; set; }

        public virtual ProductionStep ProductionStep { get; set; }
        public virtual ICollection<ProductionAssignmentDetail> ProductionAssignmentDetail { get; set; }
        public virtual ICollection<ProductionConsumMaterial> ProductionConsumMaterial { get; set; }
        public virtual ICollection<ProductionHandover> ProductionHandoverProductionAssignment { get; set; }
        public virtual ICollection<ProductionHandover> ProductionHandoverProductionAssignmentNavigation { get; set; }
        public virtual ICollection<ProductionScheduleTurnShift> ProductionScheduleTurnShift { get; set; }
    }
}
