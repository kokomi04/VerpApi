using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourcePartRequestDetail
    {
        public long OutsourcePartRequestDetailId { get; set; }
        public long OutsourcePartRequestId { get; set; }
        public int ProductId { get; set; }
        public string PathProductIdInBom { get; set; }
        public decimal Quantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
