using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceOrderExcess
    {
        public long OutsourceOrderExcessId { get; set; }
        public long OutsourceOrderId { get; set; }
        public string Title { get; set; }
        public int UnitId { get; set; }
        public string Specification { get; set; }
        public decimal Quantity { get; set; }
        public int? DecimalPlace { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual OutsourceOrder OutsourceOrder { get; set; }
    }
}
