﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceOrderExcess
    {
        public long OutsourceOrderExcessId { get; set; }
        public long OutsourceOrderId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual OutsourceOrder OutsourceOrder { get; set; }
    }
}
