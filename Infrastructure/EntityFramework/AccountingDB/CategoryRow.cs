using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryRow
    {
        public CategoryRow()
        {
            CategoryRowValues = new HashSet<CategoryRowValue>();
        }
        public int CategoryRowId { get; set; }
        public int CategoryId { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int? ParentCategoryRowId { get; set; }

        public virtual Category Category { get; set; }
        public virtual CategoryRow ParentCategoryRow { get; set; }

        public virtual ICollection<CategoryRow> ChildCategoryRows { get; set; }
        public virtual ICollection<CategoryRowValue> CategoryRowValues { get; set; }
    }
}
