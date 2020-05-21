using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryArea
    {
        public CategoryArea()
        {
            CategoryField = new HashSet<CategoryField>();
        }

        public int CategoryAreaId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryAreaCode { get; set; }
        public string Title { get; set; }
        public int SortOrder { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<CategoryField> CategoryField { get; set; }
    }
}
