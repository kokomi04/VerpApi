using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class OrderProductMaterialHistoryInput
    {
        public IList<int> ProductIds { get; set; }
        public IList<string> OrderCodes { get; set; }
    }
}
