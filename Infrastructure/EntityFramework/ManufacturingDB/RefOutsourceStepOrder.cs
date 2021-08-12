using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RefOutsourceStepOrder
    {
        public long? OutsourceRequestId { get; set; }
        public long? ProductionStepLinkDataId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public string PurchaseOrderCode { get; set; }
        public long PurchaseOrderId { get; set; }
    }
}
