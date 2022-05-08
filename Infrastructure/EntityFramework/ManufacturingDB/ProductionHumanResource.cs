using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionHumanResource
    {
        public long ProductionHumanResourceId { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }
        public DateTime? Date { get; set; }
        public decimal OfficeWorkDay { get; set; }
        public decimal OvertimeWorkDay { get; set; }
        public string Note { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
