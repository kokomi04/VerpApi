using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingRequestInput
    {
        public string PurchasingRequestCode { get; set; }
        public string OrderCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }        
        public List<PurchasingRequestInputDetail> Details { set; get; }
    }

    public class PurchasingRequestInputDetail
    {
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
    }
}
