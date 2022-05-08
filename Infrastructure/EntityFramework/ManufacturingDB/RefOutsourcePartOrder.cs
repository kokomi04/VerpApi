using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RefOutsourcePartOrder
    {
        public long? OutsourceRequestId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public string PurchaseOrderCode { get; set; }
        public long PurchaseOrderId { get; set; }
    }
}
