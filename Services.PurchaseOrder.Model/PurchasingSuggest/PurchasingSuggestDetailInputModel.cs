using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.PurchaseOrder.Model.PurchasingSuggest
{
    public class PurchasingSuggestDetailInputModel
    {
        public int CustomerId { get; set; }

        /// <summary>
        /// Mã yêu cầu VT HH
        /// </summary>
        public string PurchasingRequestCode { get; set; }

        public int ProductId { get; set; }
        
        public int PrimaryUnitId { get; set; }
        
        public decimal PrimaryQuantity { get; set; }

        public decimal PrimaryUnitPrice { get; set; }

        /// <summary>
        /// Thuế
        /// </summary>
        public decimal Tax { get; set; }

    }
}
