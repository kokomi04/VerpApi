using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryRowValue : BaseEntity
    {
        public CategoryRowValue()
        {
            DestCategoryRowValue = new HashSet<CategoryRowValue>();
        }
        public int CategoryRowId { get; set; }
        public int CategoryFieldId { get; set; }
        public int CategoryRowValueId { get; set; }
        public string Value { get; set; }
        public long ValueInNumber { get; set; }
        public int? ReferenceCategoryRowValueId { get; set; }
        public virtual CategoryRow CategoryRow { get; set; }
        public virtual CategoryField CategoryField { get; set; }

        public virtual CategoryRowValue SourceCategoryRowValue { get; set; }
        public virtual ICollection<CategoryRowValue> DestCategoryRowValue { get; set; }

        // public virtual CategoryValue CategoryValue { get; set; }
    }
}
