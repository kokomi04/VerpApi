using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class RefInventory
{
    public long InventoryId { get; set; }

    public string InventoryCode { get; set; }

    public int InventoryTypeId { get; set; }

    public int InventoryActionId { get; set; }

    public DateTime Date { get; set; }
}
