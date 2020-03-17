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
        
        public List<PurchasingSuggestInputDetail> Details { set; get; }
    }

    public class PurchasingSuggestInputDetail
    {
        public int? CustomerId { get; set; }
        public IList<long> PurchasingRequestIds { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? PrimaryUnitPrice { get; set; }
        public decimal? Tax { get; set; }
    }
}
