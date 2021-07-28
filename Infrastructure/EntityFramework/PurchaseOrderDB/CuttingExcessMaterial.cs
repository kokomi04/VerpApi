using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class CuttingExcessMaterial
    {
        public long CuttingWorkSheetId { get; set; }
        public string ExcessMaterial { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal WorkpieceQuantity { get; set; }
        public string Note { get; set; }
        public string Specification { get; set; }

        public virtual CuttingWorkSheet CuttingWorkSheet { get; set; }
    }
}
