using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryRowValue
    {
        public CategoryRowValue()
        {
            InverseReferenceCategoryRowValue = new HashSet<CategoryRowValue>();
        }

        public int CategoryRowId { get; set; }
        public int CategoryFieldId { get; set; }
        public int CategoryRowValueId { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string Value { get; set; }
        public long ValueInNumber { get; set; }
        public int? ReferenceCategoryRowValueId { get; set; }

        public virtual CategoryField CategoryField { get; set; }
        public virtual CategoryRow CategoryRow { get; set; }
        public virtual CategoryRowValue ReferenceCategoryRowValue { get; set; }
        public virtual ICollection<CategoryRowValue> InverseReferenceCategoryRowValue { get; set; }
    }
}
