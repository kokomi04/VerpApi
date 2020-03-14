using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingSuggestFile
    {
        public long PurchasingSuggestId { get; set; }
        public long FileId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
