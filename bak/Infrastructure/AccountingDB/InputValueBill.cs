﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputValueBill
    {
        public InputValueBill()
        {
            InputValueRow = new HashSet<InputValueRow>();
        }

        public long InputValueBillId { get; set; }
        public int InputTypeId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual InputType InputType { get; set; }
        public virtual ICollection<InputValueRow> InputValueRow { get; set; }
    }
}