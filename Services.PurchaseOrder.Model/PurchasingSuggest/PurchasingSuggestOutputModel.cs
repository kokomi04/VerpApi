using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingSuggest
{
    public class PurchasingSuggestOutputModel
    {
        public long PurchasingSuggestId { get; set; }
        public string PurchasingSuggestCode { get; set; }
        public string OrderCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }
        
        public int Status { set; get; }

        public int RejectCount { set; get; }        

        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }

        public int? CensorByUserId { get; set; }

        public long CensorDatetimeUtc { get; set; }

        public List<PurchasingSuggestDetailOutputModel> DetailList { set; get; }
    }
}
