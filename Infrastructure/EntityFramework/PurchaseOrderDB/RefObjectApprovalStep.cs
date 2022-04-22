using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class RefObjectApprovalStep
    {
        public int ObjectApprovalStepId { get; set; }
        public int ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public int ObjectApprovalStepTypeId { get; set; }
        public bool IsEnable { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
