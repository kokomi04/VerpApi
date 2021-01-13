using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourcePartRequest
    {
        public OutsourcePartRequest()
        {
            OutsourcePartRequestDetail = new HashSet<OutsourcePartRequestDetail>();
        }

        public long OutsourcePartRequestId { get; set; }
        public string OutsourcePartRequestCode { get; set; }
        public long ProductionOrderDetailId { get; set; }
        public DateTime OutsourcePartRequestFinishDate { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public bool MarkInvalid { get; set; }

        public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
        public virtual ICollection<OutsourcePartRequestDetail> OutsourcePartRequestDetail { get; set; }
    }
}
