using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherTypeGroup
    {
        public VoucherTypeGroup()
        {
            VoucherType = new HashSet<VoucherType>();
        }

        public int VoucherTypeGroupId { get; set; }
        public string VoucherTypeGroupName { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<VoucherType> VoucherType { get; set; }
    }
}
