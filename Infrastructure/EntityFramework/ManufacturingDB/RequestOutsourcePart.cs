﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RequestOutsourcePart
    {
        public RequestOutsourcePart()
        {
            RequestOutsourcePartDetail = new HashSet<RequestOutsourcePartDetail>();
        }

        public int RequestOutsourcePartId { get; set; }
        public string RequestOutsourcePartCode { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public DateTime DateRequiredComplete { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
        public virtual ICollection<RequestOutsourcePartDetail> RequestOutsourcePartDetail { get; set; }
    }
}
