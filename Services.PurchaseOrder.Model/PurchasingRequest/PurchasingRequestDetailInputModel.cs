using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingRequest
{
    public class PurchasingRequestDetailInputModel
    {        
        public int ProductId { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
      
    }
}
