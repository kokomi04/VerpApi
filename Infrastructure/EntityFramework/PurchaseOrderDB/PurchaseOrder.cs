using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchaseOrder
    {
        public PurchaseOrder()
        {
            PurchaseOrderDetail = new HashSet<PurchaseOrderDetail>();
        }

        public long PurchaseOrderId { get; set; }
        public string PurchaseOrderCode { get; set; }
        public long? PoAssignmentId { get; set; }
        public long? PurchasingSuggestId { get; set; }
        public int CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public string DeliveryDestination { get; set; }
        public string Content { get; set; }
        public string AdditionNote { get; set; }
        public int PurchaseOrderStatusId { get; set; }
        public bool? IsApproved { get; set; }
        public int? PoProcessStatusId { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalMoney { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CensorByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? CensorDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual PoAssignment PoAssignment { get; set; }
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
    }
}
