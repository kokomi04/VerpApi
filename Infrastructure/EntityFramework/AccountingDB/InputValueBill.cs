using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputValueBill : BaseEntity
    {
        public InputValueBill()
        {
            InputValueRows = new HashSet<InputValueRow>();
        }

        public long InputValueBillId { get; set; }
        public int InputTypeId { get; set; }

        public virtual ICollection<InputValueRow> InputValueRows { get; set; }

    }
}
