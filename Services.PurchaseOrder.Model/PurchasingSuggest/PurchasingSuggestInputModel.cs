using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingSuggest
{
    public class PurchasingSuggestInputModel
    {
        public string PurchasingSuggestCode { get; set; }
       
        public string OrderCode { get; set; }
        
        public long Date { get; set; }
        
        public string Content { get; set; }      
        
        public List<PurchasingSuggestDetailInputModel> DetailList { set; get; }
    }
}
