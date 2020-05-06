using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputValueRow
    {
        public InputValueRow()
        {
            InputValueRowVersion = new HashSet<InputValueRowVersion>();
        }

        public long InputValueRowId { get; set; }
        public long InputValueBillId { get; set; }
        public int InputAreaId { get; set; }
        public long LastestInputValueRowVersionId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual InputArea InputArea { get; set; }
        public virtual InputValueBill InputValueBill { get; set; }
        public virtual ICollection<InputValueRowVersion> InputValueRowVersion { get; set; }
    }
}
