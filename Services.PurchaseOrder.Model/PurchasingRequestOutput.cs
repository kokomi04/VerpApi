using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingRequestOutputList
    {
        public long PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }
        public string OrderCode { get; set; }
        public EnumPurchasingRequestStatus PurchasingRequestStatusId { get; set; }
        public bool? IsApproved { get; set; }
        public EnumPoProcessStatus? PoProcessStatusId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CensorByUserId { get; set; }

        public long? CensorDatetimeUtc { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }


    public class PurchasingRequestOutput: PurchasingRequestOutputList
    {
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public IList<long> FileIds { get; set; }
        public List<PurchasingRequestOutputDetail> Details { set; get; }
    }


    public class PurchasingRequestOutputDetail: PurchasingRequestInputDetail
    {
        public long PurchasingRequestDetailId { get; set; }
    }
}
