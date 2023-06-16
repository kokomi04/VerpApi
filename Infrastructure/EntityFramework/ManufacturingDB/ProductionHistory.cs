using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionHistory
    {
        public long ProductionHistoryId { get; set; }
        public int DepartmentId { get; set; }
        public decimal ProductionQuantity { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public long ProductionStepId { get; set; }
        public DateTime? Date { get; set; }
        public long ProductionOrderId { get; set; }
        public string Note { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public decimal? OvertimeProductionQuantity { get; set; }
        public long? ProductionHandoverReceiptId { get; set; }
        public int RowIndex { get; set; }

        public virtual ProductionHandoverReceipt ProductionHandoverReceipt { get; set; }
        public virtual ProductionStep ProductionStep { get; set; }
    }
}
