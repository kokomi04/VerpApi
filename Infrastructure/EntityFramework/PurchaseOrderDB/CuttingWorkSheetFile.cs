﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class CuttingWorkSheetFile
{
    public long CuttingWorkSheetId { get; set; }

    public long FileId { get; set; }

    public virtual CuttingWorkSheet CuttingWorkSheet { get; set; }
}
