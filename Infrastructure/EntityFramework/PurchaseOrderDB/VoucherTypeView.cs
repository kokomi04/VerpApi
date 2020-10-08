using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherTypeView
    {
        public VoucherTypeView()
        {
            VoucherTypeViewField = new HashSet<VoucherTypeViewField>();
        }

        public int VoucherTypeViewId { get; set; }
        public string VoucherTypeViewName { get; set; }
        public int VoucherTypeId { get; set; }
        public int? UserId { get; set; }
        public bool IsDefault { get; set; }
        public int? Columns { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual VoucherType VoucherType { get; set; }
        public virtual ICollection<VoucherTypeViewField> VoucherTypeViewField { get; set; }
    }
}
