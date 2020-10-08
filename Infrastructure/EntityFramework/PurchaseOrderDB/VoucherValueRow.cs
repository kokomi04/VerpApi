using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherValueRow
    {
        public long FId { get; set; }
        public int VoucherTypeId { get; set; }
        public long SaleBillFId { get; set; }
        public int BillVersion { get; set; }
        public bool IsBillEntry { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string SystemLog { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
