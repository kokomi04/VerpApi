﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionHandoverReceipt
{
    public long ProductionHandoverReceiptId { get; set; }

    public string ProductionHandoverReceiptCode { get; set; }

    public int HandoverStatusId { get; set; }

    public int? AcceptByUserId { get; set; }

    public int SubsidiaryId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<ProductionHandover> ProductionHandover { get; set; } = new List<ProductionHandover>();

    public virtual ICollection<ProductionHistory> ProductionHistory { get; set; } = new List<ProductionHistory>();
}
