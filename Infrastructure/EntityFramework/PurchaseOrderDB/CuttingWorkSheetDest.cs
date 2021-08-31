using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class CuttingWorkSheetDest
    {
        public long CuttingWorkSheetId { get; set; }
        public int ProductId { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal WorkpieceQuantity { get; set; }
        public string Note { get; set; }

        public virtual CuttingWorkSheet CuttingWorkSheet { get; set; }
    }
}
