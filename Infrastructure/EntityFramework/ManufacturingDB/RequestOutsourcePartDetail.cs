using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RequestOutsourcePartDetail
    {
        public long RequestOutsourcePartDetailId { get; set; }
        public long RequestOutsourcePartId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual RequestOutsourcePart RequestOutsourcePart { get; set; }
    }
}
