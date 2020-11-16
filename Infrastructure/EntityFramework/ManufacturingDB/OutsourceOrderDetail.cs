﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceOrderDetail
    {
        public long OutsourceOrderDetailId { get; set; }
        public long OutsoureOrderId { get; set; }
        public int ObjectId { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual OutsourceOrder OutsoureOrder { get; set; }
    }
}
