using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryRow : BaseEntity
    {
        public CategoryRow()
        {
            CategoryRowValues = new HashSet<CategoryRowValue>();
        }
        public int CategoryRowId { get; set; }
        public int CategoryId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<CategoryRowValue> CategoryRowValues { get; set; }
    }
}
