using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingRequest
{
    public class PurchasingRequestOutputModel
    {
        public long PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }
        public string OrderCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }
        public bool IsApproved { get; set; }
        //public bool IsDeleted { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public long CreatedDatetime { get; set; }
        public long UpdatedDatetime { get; set; }

        public List<PurchasingRequestDetailOutputModel> DetailList { set; get; }
    }
}
