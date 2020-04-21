using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryValue : BaseEntity
    {
        public CategoryValue()
        {
            //CategoryRowValues = new HashSet<CategoryRowValue>();
        }

        public int CategoryValueId { get; set; }
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }
        public bool IsDefault { get; set; }

        //public virtual ICollection<CategoryRowValue> CategoryRowValues { get; set; }

        public virtual CategoryField CategoryField { get; set; }
    }
}
