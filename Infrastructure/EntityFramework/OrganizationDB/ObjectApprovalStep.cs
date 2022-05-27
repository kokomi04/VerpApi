using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ObjectApprovalStep
    {
        public int ObjectApprovalStepId { get; set; }
        public int ObjectTypeId { get; set; }
        public int ObjectId { get; set; }
        public int ObjectApprovalStepTypeId { get; set; }
        public bool IsEnable { get; set; }
        public string ObjectFieldEnable { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
    }
}
