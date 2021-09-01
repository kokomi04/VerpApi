using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingSuggest
    {
        public PurchasingSuggest()
        {
            PoAssignment = new HashSet<PoAssignment>();
            PurchasingSuggestDetail = new HashSet<PurchasingSuggestDetail>();
            PurchasingSuggestFile = new HashSet<PurchasingSuggestFile>();
        }

        public long PurchasingSuggestId { get; set; }
        public int SubsidiaryId { get; set; }
        public string PurchasingSuggestCode { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public int PurchasingSuggestStatusId { get; set; }
        public bool? IsApproved { get; set; }
        public int? PoProcessStatusId { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CensorByUserId { get; set; }
        public DateTime? CensorDatetimeUtc { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<PoAssignment> PoAssignment { get; set; }
        public virtual ICollection<PurchasingSuggestDetail> PurchasingSuggestDetail { get; set; }
        public virtual ICollection<PurchasingSuggestFile> PurchasingSuggestFile { get; set; }
    }
}
