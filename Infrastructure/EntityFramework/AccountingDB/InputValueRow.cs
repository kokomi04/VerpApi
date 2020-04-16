using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputValueRow : BaseEntity
    {
        public InputValueRow()
        {
            InputValueRowVersions = new HashSet<InputValueRowVersion>();
        }

        public long InputValueRowId { get; set; }
        public long InputValueBillId { get; set; }
        public long LastestInputValueRowVersionId { get; set; }
        public int InputAreaId { get; set; }

        public virtual InputValueBill InputValueBill { get; set; }
        public virtual ICollection<InputValueRowVersion> InputValueRowVersions { get; set; }
    }
}
