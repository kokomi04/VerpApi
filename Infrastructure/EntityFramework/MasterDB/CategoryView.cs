using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class CategoryView
    {
        public CategoryView()
        {
            CategoryViewField = new HashSet<CategoryViewField>();
        }

        public int CategoryViewId { get; set; }
        public string CategoryViewName { get; set; }
        public int CategoryId { get; set; }
        public bool IsDefault { get; set; }
        public int? SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<CategoryViewField> CategoryViewField { get; set; }
    }
}
