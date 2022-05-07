﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionConsumMaterial
    {
        public ProductionConsumMaterial()
        {
            ProductionConsumMaterialDetail = new HashSet<ProductionConsumMaterialDetail>();
        }

        public long ProductionConsumMaterialId { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public long ProductionOrderId { get; set; }

        public virtual ProductionAssignment ProductionAssignment { get; set; }
        public virtual ProductionStep ProductionStep { get; set; }
        public virtual ICollection<ProductionConsumMaterialDetail> ProductionConsumMaterialDetail { get; set; }
    }
}
