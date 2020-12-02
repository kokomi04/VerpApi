using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrder
    {
        public ProductionOrder()
        {
            OutsourceStepRequest = new HashSet<OutsourceStepRequest>();
            ProductionOrderDetail = new HashSet<ProductionOrderDetail>();
        }

        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDraft { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ICollection<OutsourceStepRequest> OutsourceStepRequest { get; set; }
        public virtual ICollection<ProductionOrderDetail> ProductionOrderDetail { get; set; }
    }
}
