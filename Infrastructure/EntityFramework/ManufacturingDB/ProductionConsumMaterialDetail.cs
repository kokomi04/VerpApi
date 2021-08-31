using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionConsumMaterialDetail
    {
        public long ProductionConsumMaterialDetailId { get; set; }
        public long ProductionConsumMaterialId { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public decimal? Quantity { get; set; }

        public virtual ProductionConsumMaterial ProductionConsumMaterial { get; set; }
    }
}
