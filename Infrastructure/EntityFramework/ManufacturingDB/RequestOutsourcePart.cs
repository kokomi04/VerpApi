using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class RequestOutsourcePart
    {
        public int RequestOutsourcePartId { get; set; }
        public string RequestOutsourcePartCode { get; set; }
        public int ProductionOrderDetailId { get; set; }
        public DateTime DateRequiredComplete { get; set; }
        public int ProductId { get; set; }
        public int Quanity { get; set; }
        public int Status { get; set; }
        public int UnitId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
    }
}
