using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherBill
    {
        public long FId { get; set; }
        public int VoucherTypeId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int LatestBillVersion { get; set; }
        public int SubsidiaryId { get; set; }
        public string BillCode { get; set; }

        public virtual VoucherType VoucherType { get; set; }
    }
}
