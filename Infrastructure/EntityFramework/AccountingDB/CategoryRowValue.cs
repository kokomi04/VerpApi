using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryRowValue : BaseEntity
    {
        public int CategoryRowId { get; set; }
        public int CategoryFieldId { get; set; }
        public int CategoryValueId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
