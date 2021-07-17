using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class CuttingWorkSheetDest
    {
        public long CuttingWorkSheetId { get; set; }
        public int ProductId { get; set; }
        public decimal ProductQuantity { get; set; }

        public virtual CuttingWorkSheet CuttingWorkSheet { get; set; }
    }
}
