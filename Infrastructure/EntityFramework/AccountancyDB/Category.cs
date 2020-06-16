using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class Category
    {
        public Category()
        {
            CategoryArea = new HashSet<CategoryArea>();
        }

        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string CategoryCode { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsTreeView { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsOutSideData { get; set; }

        public virtual OutSideDataConfig OutSideDataConfig { get; set; }
        public virtual ICollection<CategoryArea> CategoryArea { get; set; }
    }
}
