using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingSuggestOutputList
    {
        public long PurchasingSuggestId { get; set; }
        public string PurchasingSuggestCode { get; set; }
        public string OrderCode { get; set; }
        public EnumPurchasingSuggestStatus PurchasingSuggestStatusId { get; set; }
        public bool? IsApproved { get; set; }
        public EnumPoProcessStatus? PoProcessStatusId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CensorByUserId { get; set; }

        public long? CensorDatetimeUtc { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
    }


    public class PurchasingSuggestOutput : PurchasingSuggestOutputList
    {
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public IList<long> FileIds { get; set; }
        public List<PurchasingSuggestDetailModel> Details { set; get; }
    }

    public class PurchasingSuggestOutputListByProduct : PurchasingSuggestOutputList
    {
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public long? PurchasingSuggestDetailId { get; set; }
        public int CustomerId { get; set; }
        public IList<long> PurchasingRequestIds { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? PrimaryUnitPrice { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }
    }

}
