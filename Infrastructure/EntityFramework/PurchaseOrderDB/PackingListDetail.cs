using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PackingListDetail
    {
        public int PackingListDetailId { get; set; }
        public int PackingListId { get; set; }
        public int VoucherValueRowId { get; set; }
        public int ActualNumber { get; set; }
        public decimal NetWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal CubicMeter { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual PackingList PackingList { get; set; }
    }
}
