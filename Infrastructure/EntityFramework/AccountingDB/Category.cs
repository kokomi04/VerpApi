using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class Category : BaseEntity
    {
        public Category()
        {
            SubCategories = new HashSet<Category>();
            CategoryFields = new HashSet<CategoryField>();
            CategoryRows = new HashSet<CategoryRow>();
        }

        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string CategoryCode { get; set; }
        public bool IsModule { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsTreeView { get; set; }

        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; }
        public virtual ICollection<CategoryField> CategoryFields { get; set; }
        public virtual ICollection<CategoryRow> CategoryRows { get; set; }
    }
}
