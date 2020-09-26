using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PoAssignment
    {
        public PoAssignment()
        {
            PoAssignmentDetail = new HashSet<PoAssignmentDetail>();
        }

        public long PoAssignmentId { get; set; }
        public int SubsidiaryId { get; set; }
        public long PurchasingSuggestId { get; set; }
        public string PoAssignmentCode { get; set; }
        public DateTime? Date { get; set; }
        public string Content { get; set; }
        public int AssigneeUserId { get; set; }
        public int PoAssignmentStatusId { get; set; }
        public bool? IsConfirmed { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PurchasingSuggest PurchasingSuggest { get; set; }
        public virtual ICollection<PoAssignmentDetail> PoAssignmentDetail { get; set; }
    }
}
