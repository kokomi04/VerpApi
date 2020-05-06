﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class InputType
    {
        public InputType()
        {
            InputArea = new HashSet<InputArea>();
            InputAreaField = new HashSet<InputAreaField>();
            InputTypeView = new HashSet<InputTypeView>();
            InputValueBill = new HashSet<InputValueBill>();
        }

        public int InputTypeId { get; set; }
        public string Title { get; set; }
        public string InputTypeCode { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<InputArea> InputArea { get; set; }
        public virtual ICollection<InputAreaField> InputAreaField { get; set; }
        public virtual ICollection<InputTypeView> InputTypeView { get; set; }
        public virtual ICollection<InputValueBill> InputValueBill { get; set; }
    }
}
