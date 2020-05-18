using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingRequest
    {
        public PurchasingRequest()
        {
            PurchasingRequestDetail = new HashSet<PurchasingRequestDetail>();
        }

        public long PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }
        public DateTime Date { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public int PurchasingRequestStatusId { get; set; }
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

        public virtual ICollection<PurchasingRequestDetail> PurchasingRequestDetail { get; set; }
    }
}
