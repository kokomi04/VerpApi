using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class CategoryValue : BaseEntity
    {
        public int CategoryValueId { get; set; }
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
    }
}
