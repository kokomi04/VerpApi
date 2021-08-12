using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RefOutsourcePartTrack
    {
        public long PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; }
        public long PurchaseOrderTrackedId { get; set; }
        public DateTime Date { get; set; }
        public long? ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
    }
}
