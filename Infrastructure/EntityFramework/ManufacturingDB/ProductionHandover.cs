﻿using System;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionHandover
    {
        public long ProductionHandoverId { get; set; }
        public int FromDepartmentId { get; set; }
        public decimal HandoverQuantity { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int Status { get; set; }
        public long FromProductionStepId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public int ToDepartmentId { get; set; }
        public long ToProductionStepId { get; set; }
        public DateTime? HandoverDatetime { get; set; }
        public long ProductionOrderId { get; set; }
        public string Note { get; set; }
        public int? AcceptByUserId { get; set; }

        public virtual ProductionStep FromProductionStep { get; set; }
        public virtual ProductionOrder ProductionOrder { get; set; }
        public virtual ProductionStep ToProductionStep { get; set; }
    }
}
