using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class CuttingWorkSheet
    {
        public CuttingWorkSheet()
        {
            CuttingExcessMaterial = new HashSet<CuttingExcessMaterial>();
            CuttingWorkSheetDest = new HashSet<CuttingWorkSheetDest>();
            CuttingWorkSheetFile = new HashSet<CuttingWorkSheetFile>();
        }

        public long CuttingWorkSheetId { get; set; }
        public long PropertyCalcId { get; set; }
        public int InputProductId { get; set; }
        public decimal InputQuantity { get; set; }
        public int SubsidiaryId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PropertyCalc PropertyCalc { get; set; }
        public virtual ICollection<CuttingExcessMaterial> CuttingExcessMaterial { get; set; }
        public virtual ICollection<CuttingWorkSheetDest> CuttingWorkSheetDest { get; set; }
        public virtual ICollection<CuttingWorkSheetFile> CuttingWorkSheetFile { get; set; }
    }
}
