using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrderType
    {
        public int PurchaseOrderTypeId { get; set; }
        public string PurchaseOrderTypeName { get; set; }
        public string Description { get; set; }
    }
}
