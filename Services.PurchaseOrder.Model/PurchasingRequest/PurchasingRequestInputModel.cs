using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingRequest
{
    public class PurchasingRequestInputModel
    {
        public string PurchasingRequestCode { get; set; }
        public string OrderCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }      
        
        public List<PurchasingRequestDetailInputModel> DetailList { set; get; }
    }
}
