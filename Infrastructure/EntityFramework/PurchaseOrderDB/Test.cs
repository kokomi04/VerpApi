using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class Test
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
    }
}
