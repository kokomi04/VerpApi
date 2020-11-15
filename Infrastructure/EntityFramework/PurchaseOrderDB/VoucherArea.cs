using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherArea
    {
        public VoucherArea()
        {
            VoucherAreaField = new HashSet<VoucherAreaField>();
        }

        public int VoucherAreaId { get; set; }
        public int VoucherTypeId { get; set; }
        public string VoucherAreaCode { get; set; }
        public string Title { get; set; }
        public bool IsMultiRow { get; set; }
        public int Columns { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string ColumnStyles { get; set; }

        public virtual VoucherType VoucherType { get; set; }
        public virtual ICollection<VoucherAreaField> VoucherAreaField { get; set; }
    }
}
