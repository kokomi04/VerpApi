using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingRequest
    {
        public long PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }
        public string OrderCode { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? CreatedDatetime { get; set; }
        public DateTime? UpdatedDatetime { get; set; }
    }
}
