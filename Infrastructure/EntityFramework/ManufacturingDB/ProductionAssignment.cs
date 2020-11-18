using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionAssignment
    {
        public long ProductionStepId { get; set; }
        public long ScheduleTurnId { get; set; }
        public int DepartmentId { get; set; }
        public int AssignmentQuantity { get; set; }
        public long ProductionStepLinkDataId { get; set; }

        public virtual ProductionStep ProductionStep { get; set; }
    }
}
