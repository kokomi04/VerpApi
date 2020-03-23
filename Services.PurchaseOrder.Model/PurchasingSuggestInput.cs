using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingSuggestInput
    {
        public string PurchasingSuggestCode { get; set; }
        public string OrderCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }
        public IList<long> FileIds { get; set; }
        public List<PurchasingSuggestDetailModel> Details { set; get; }
    }

    public class PurchasingSuggestDetailModel
    {
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
