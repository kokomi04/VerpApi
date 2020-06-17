using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class InputValueRow
    {
        public long InputValueRowId { get; set; }
        public int InputTypeId { get; set; }
        public long InputValueBillId { get; set; }
        public long BillVersionId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string ChungTu { get; set; }

        public virtual InputType InputType { get; set; }
        public virtual InputValueBill InputValueBill { get; set; }
    }
}
