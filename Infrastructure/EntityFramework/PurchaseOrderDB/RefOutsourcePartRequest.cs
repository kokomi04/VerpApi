using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class RefOutsourcePartRequest
    {
        public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public decimal? Quantity { get; set; }
        public DateTime? OutsourcePartRequestDetailFinishDate { get; set; }
    }
}
