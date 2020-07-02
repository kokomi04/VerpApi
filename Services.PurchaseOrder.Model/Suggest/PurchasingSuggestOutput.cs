using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingSuggestBasic
    {
        public long PurchasingSuggestId { get; set; }
        public string PurchasingSuggestCode { get; set; }
    }

    public class PurchasingSuggestOutputList: PurchasingSuggestBasic
    {
        public long Date { get; set; }      
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
        public List<PurchasingSuggestDetailOutputModel> Details { set; get; }
    }

    public class PurchasingSuggestDetailOutputModel : PurchasingSuggestDetailInputModel
    {     
        public PurchasingRequestDetailInfo RequestDetail { get; set; }
    }

    public class PurchasingSuggestOutputListByProduct : PurchasingSuggestOutputList
    {
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public long? PurchasingSuggestDetailId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? PrimaryUnitPrice { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }

        public PurchasingRequestDetailInfo RequestDetail { get; set; }

    }

}
