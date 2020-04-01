using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryRow : BaseEntity
    {
        public int CategoryRowId { get; set; }
        public int CategoryId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
