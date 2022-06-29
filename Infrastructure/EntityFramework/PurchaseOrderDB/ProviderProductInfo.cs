using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class ProviderProductInfo
    {
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string ProviderProductName { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
