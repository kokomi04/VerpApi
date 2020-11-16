﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrderDetail
    {
        public ProductionOrderDetail()
        {
            ProductionSchedule = new HashSet<ProductionSchedule>();
            ProductionStepOrder = new HashSet<ProductionStepOrder>();
            RequestOutsourcePart = new HashSet<RequestOutsourcePart>();
        }

        public int ProductionOrderDetailId { get; set; }
        public int ProductionOrderId { get; set; }
        public int ProductId { get; set; }
        public int? Quantity { get; set; }
        public int? ReserveQuantity { get; set; }
        public string Note { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public long? OrderDetailId { get; set; }
        public int? Status { get; set; }
        public int SubsidiaryId { get; set; }

        public virtual ProductionOrder ProductionOrder { get; set; }
        public virtual ICollection<ProductionSchedule> ProductionSchedule { get; set; }
        public virtual ICollection<ProductionStepOrder> ProductionStepOrder { get; set; }
        public virtual ICollection<RequestOutsourcePart> RequestOutsourcePart { get; set; }
    }
}
