using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStagesDetail
    {
        public int ProductionStagesDetailId { get; set; }
        public int ProductionStagesId { get; set; }
        public int InOutStagesType { get; set; }
        public int ProductType { get; set; }
        public int ProductId { get; set; }
        public decimal ActualNumber { get; set; }
        public int UnitId { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SortOrder { get; set; }
    }
}
