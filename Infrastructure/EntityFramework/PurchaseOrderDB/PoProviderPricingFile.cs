using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PoProviderPricingFile
    {
        public long PoProviderPricingFileId { get; set; }
        public long PoProviderPricingId { get; set; }
        public long FileId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PoProviderPricing PoProviderPricing { get; set; }
    }
}
