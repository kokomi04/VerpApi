﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class FormType : BaseEntity
    {
        public FormType()
        {
            CategoryFields = new HashSet<CategoryField>();
            InputAreaFields = new HashSet<InputAreaField>();
        }

        public int FormTypeId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }

        public virtual ICollection<CategoryField> CategoryFields { get; set; }
        public virtual ICollection<InputAreaField> InputAreaFields { get; set; }
    }
}
