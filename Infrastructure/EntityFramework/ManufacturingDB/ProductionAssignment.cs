using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionAssignment
    {
        public ProductionAssignment()
        {
            ProductionScheduleTurnShift = new HashSet<ProductionScheduleTurnShift>();
        }

        public long ProductionStepId { get; set; }
        public long ScheduleTurnId { get; set; }
        public int DepartmentId { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }

        public virtual ProductionStep ProductionStep { get; set; }
        public virtual ICollection<ProductionScheduleTurnShift> ProductionScheduleTurnShift { get; set; }
    }
}
