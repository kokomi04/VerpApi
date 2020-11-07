using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrder
    {
        public ProductionOrder()
        {
            ProductionOrderDetail = new HashSet<ProductionOrderDetail>();
        }

        public int ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public DateTime VoucherDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<ProductionOrderDetail> ProductionOrderDetail { get; set; }
    }
}
