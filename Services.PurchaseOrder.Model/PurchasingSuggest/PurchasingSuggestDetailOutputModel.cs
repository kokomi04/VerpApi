using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingSuggest
{
    public class PurchasingSuggestDetailOutputModel
    {
        public long PurchasingSuggestDetailId { get; set; }
        
        public long PurchasingSuggestId { get; set; }

        public int CustomerId { set; get; }

        public string CustomerCode { set; get; }

        public string CustomerName { set; get; }

        public string PurchasingRequestCode { set; get; }

        public int ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public int PrimaryUnitId { get; set; }

        public string PrimaryUnitName { get; set; }

        public decimal PrimaryQuantity { get; set; }

        public decimal PrimaryUnitPrice { set; get; }

        public decimal Tax { set; get; }

        public DateTime? CreatedDatetime { get; set; }
        
        public DateTime? UpdatedDatetime { get; set; }
        
        
    }
}
