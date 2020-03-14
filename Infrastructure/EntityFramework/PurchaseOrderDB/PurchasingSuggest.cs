using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PurchasingSuggest
    {
        public long PurchasingSuggestId { get; set; }
        public string PurchasingSuggestCode { get; set; }
        public string OrderCode { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public int Status { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public int? CensorByUserId { get; set; }
        public DateTime? CensorDatetimeUtc { get; set; }
        public DateTime? CreatedDatetimeUtc { get; set; }
        public DateTime? UpdatedDatetimeUtc { get; set; }
    }
}
