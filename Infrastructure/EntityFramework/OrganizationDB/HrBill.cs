using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class HrBill
    {
        public long FId { get; set; }
        public int HrTypeId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int LatestBillVersion { get; set; }
        public int SubsidiaryId { get; set; }
        public string BillCode { get; set; }

        public virtual HrType HrType { get; set; }
    }
}
