using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionHandover
    {
        public long ProductionHandoverId { get; set; }
        public long ScheduleTurnId { get; set; }
        public int FromDepartmentId { get; set; }
        public decimal HandoverQuantity { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int Status { get; set; }
        public long FromProductionStepId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public int ToDepartmentId { get; set; }
        public long ToProductionStepId { get; set; }
        public DateTime? HandoverDatetime { get; set; }

        public virtual ProductionAssignment ProductionAssignment { get; set; }
        public virtual ProductionAssignment ProductionAssignmentNavigation { get; set; }
    }
}
