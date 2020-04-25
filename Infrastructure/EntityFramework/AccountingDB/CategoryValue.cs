using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryValue 
    {
        public CategoryValue()
        {
        }

        public int CategoryValueId { get; set; }
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        //public virtual ICollection<CategoryRowValue> CategoryRowValues { get; set; }

        public virtual CategoryField CategoryField { get; set; }
    }
}
