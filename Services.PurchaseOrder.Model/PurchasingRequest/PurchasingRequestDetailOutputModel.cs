using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingRequest
{
    public class PurchasingRequestDetailOutputModel
    {
        public long PurchasingRequestDetailId { get; set; }
        
        public long PurchasingRequestId { get; set; }
        
        public int ProductId { get; set; }
        
        public int PrimaryUnitId { get; set; }
        
        public decimal PrimaryQuantity { get; set; }
        
        public DateTime? CreatedDatetime { get; set; }
        
        public DateTime? UpdatedDatetime { get; set; }
        
        public string ProductName { set; get; }

        public string ProductCode { set; get; }

        public string PrimaryUnitName { set; get; }
    }
}
