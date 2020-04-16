using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountingDB
{
    public partial class BaseEntity
    {
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
