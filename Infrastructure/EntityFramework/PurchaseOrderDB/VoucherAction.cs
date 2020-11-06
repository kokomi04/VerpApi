using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class VoucherAction
    {
        public int VoucherActionId { get; set; }
        public int VoucherTypeId { get; set; }
        public string Title { get; set; }
        public string VoucherActionCode { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string SqlAction { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
        public string Style { get; set; }
        //public int Position { get; set; }
        public string JsVisible { get; set; }

        public virtual VoucherType VoucherType { get; set; }
    }
}
