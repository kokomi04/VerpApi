using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class CuttingWorkSheet
{
    public long CuttingWorkSheetId { get; set; }

    public long PropertyCalcId { get; set; }

    public int InputProductId { get; set; }

    public decimal InputQuantity { get; set; }

    public virtual ICollection<CuttingExcessMaterial> CuttingExcessMaterial { get; set; } = new List<CuttingExcessMaterial>();

    public virtual ICollection<CuttingWorkSheetDest> CuttingWorkSheetDest { get; set; } = new List<CuttingWorkSheetDest>();

    public virtual ICollection<CuttingWorkSheetFile> CuttingWorkSheetFile { get; set; } = new List<CuttingWorkSheetFile>();

    public virtual PropertyCalc PropertyCalc { get; set; }
}
