using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductMaterialsConsumption
    {
        public long ProductMaterialsConsumptionId { get; set; }
        public int ProductId { get; set; }
        public int MaterialsConsumptionId { get; set; }
        public decimal Quantity { get; set; }
        public int? StepId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
