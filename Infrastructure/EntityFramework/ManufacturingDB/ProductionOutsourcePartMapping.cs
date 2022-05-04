using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOutsourcePartMapping
    {
        public long ProductionOutsourcePartMappingId { get; set; }
        public long ContainerId { get; set; }
        public long OutsourcePartRequestDetailId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsDefault { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
