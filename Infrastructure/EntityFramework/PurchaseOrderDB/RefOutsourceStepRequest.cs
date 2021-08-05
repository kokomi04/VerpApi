using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class RefOutsourceStepRequest
    {
        public long OutsourceStepRequestId { get; set; }
        public string OutsourceStepRequestCode { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionStepId { get; set; }
        public int? StepId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsImportant { get; set; }
        public int ProductionStepLinkDataRoleTypeId { get; set; }
        public DateTime OutsourceStepRequestFinishDate { get; set; }
    }
}
