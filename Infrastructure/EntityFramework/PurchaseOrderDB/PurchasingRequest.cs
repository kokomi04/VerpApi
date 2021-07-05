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
        public int SubsidiaryId { get; set; }
        public string PurchasingRequestCode { get; set; }
        public DateTime Date { get; set; }
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
        public long? OrderDetailId { get; set; }
        public int PurchasingRequestTypeId { get; set; }
        public decimal? OrderDetailQuantity { get; set; }
        public decimal? OrderDetailRequestQuantity { get; set; }
        public long? ProductionOrderId { get; set; }
        public long? MaterialCalcId { get; set; }

        public virtual MaterialCalc MaterialCalc { get; set; }
        public virtual ICollection<PurchasingRequestDetail> PurchasingRequestDetail { get; set; }
    }
}
